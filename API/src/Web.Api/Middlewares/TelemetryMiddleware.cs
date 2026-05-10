// TelemetryMiddleware.cs
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
namespace Web.Api.Middlewares;

public class TelemetryMiddleware
{
    private const string OperationSuccessKey = "telemetry.operation.success";
    private const string OperationErrorCode = "telemetry.operation.error.code";
    private const string OperationErrorType = "telemetry.operation.error.type";
    private const string OperationErrorMsg = "telemetry.operation.error.message";

    private static readonly ActivitySource _source = new("MyApi");
    private static readonly Meter _meter = new("MyApi");

    private static readonly Counter<int> _httpRequests =
        _meter.CreateCounter<int>("http.requests.total");

    private static readonly Histogram<double> _httpDuration =
        _meter.CreateHistogram<double>("http.request.duration", unit: "ms");

    private static readonly Counter<int> _operationTotal =
        _meter.CreateCounter<int>("operations.total");

    private static readonly Counter<int> _operationErrors =
        _meter.CreateCounter<int>("operations.errors");

    private readonly RequestDelegate _next;
    public TelemetryMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var sw = Stopwatch.StartNew();

        using var activity = _source.StartActivity($"{method} {path}");
        activity?.SetTag("http.method", method);
        activity?.SetTag("http.path", path);
        activity?.SetTag("http.host", context.Request.Host.Value);

        await _next(context);

        sw.Stop();
        var status = context.Response.StatusCode;

        // ── HTTP Metrics ──────────────────────────────────────────
        var httpTags = new TagList
        {
            { "method", method },
            { "path",   path   },
            { "status", status }
        };

        _httpRequests.Add(1, httpTags);
        _httpDuration.Record(sw.Elapsed.TotalMilliseconds, httpTags);

        activity?.SetTag("http.status_code", status);
        activity?.SetTag("http.duration_ms", sw.Elapsed.TotalMilliseconds);

        // ── Result outcome (set by DiagnosticObserver below) ──────
        var isSuccess = context.Items[OperationSuccessKey] as bool?;
        var errorCode = context.Items[OperationErrorCode]?.ToString();
        var errorType = context.Items[OperationErrorType]?.ToString();
        var errorMsg = context.Items[OperationErrorMsg]?.ToString();

        if (isSuccess.HasValue)
        {
            _operationTotal.Add(1, new TagList { { "success", isSuccess.Value } });
            activity?.SetTag("result.success", isSuccess.Value);

            if (!isSuccess.Value)
            {
                _operationErrors.Add(1, new TagList
                {
                    { "error.code", errorCode ?? "UNKNOWN" },
                    { "error.type", errorType ?? "Failure" }
                });

                activity?.SetTag("result.error.code", errorCode);
                activity?.SetTag("result.error.type", errorType);
                activity?.SetTag("result.error.message", errorMsg);
                activity?.SetStatus(ActivityStatusCode.Error, errorMsg);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
        }
        else
        {
            activity?.SetStatus(status >= 400
                ? ActivityStatusCode.Error
                : ActivityStatusCode.Ok);
        }
    }

    // ── Subscribes to ASP.NET Core diagnostic events ──────────────
    internal sealed class MvcDiagnosticObserver : IObserver<DiagnosticListener>
    {
        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == "Microsoft.AspNetCore")
                listener.Subscribe(new AspNetCoreEventObserver());
        }
        public void OnError(Exception _) { }
        public void OnCompleted() { }
    }

    // ── Fires on every controller action result ───────────────────
    internal sealed class AspNetCoreEventObserver : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> pair)
        {
            if (pair.Key != "Microsoft.AspNetCore.Mvc.BeforeActionResult") return;

            var payload = pair.Value;
            if (payload is null) return;

            var payloadType = payload.GetType();

            var httpContext = payloadType
                .GetProperty("httpContext")
                ?.GetValue(payload) as HttpContext;

            var actionResult = payloadType
                .GetProperty("result")
                ?.GetValue(payload);

            if (httpContext is null || actionResult is null) return;

            // Only ObjectResult carries a value (Ok, NotFound, BadRequest, etc.)
            if (actionResult is not ObjectResult { Value: not null } objectResult) return;

            var valueType = objectResult.Value.GetType();
            var isSuccessProp = valueType.GetProperty("IsSuccess");

            // Not a Result<T>, skip
            if (isSuccessProp is null) return;

            var isSuccess = (bool)isSuccessProp.GetValue(objectResult.Value)!;
            httpContext.Items[OperationSuccessKey] = isSuccess;

            if (!isSuccess)
            {
                var error = valueType.GetProperty("Error")?.GetValue(objectResult.Value);
                if (error is null) return;

                var errorType = error.GetType();
                httpContext.Items[OperationErrorCode] = errorType.GetProperty("Code")?.GetValue(error)?.ToString();
                httpContext.Items[OperationErrorType] = errorType.GetProperty("Type")?.GetValue(error)?.ToString();
                httpContext.Items[OperationErrorMsg] = errorType.GetProperty("Message")?.GetValue(error)?.ToString();
            }
        }

        public void OnError(Exception _) { }
        public void OnCompleted() { }
    }
}

// ── Must be in a non-generic static class ─────────────────────────
public static class TelemetryExtensions
{
    // Store providers statically so GC never collects them
    private static TracerProvider? _tracerProvider;
    private static MeterProvider? _meterProvider;

    public static void UseTelemetry(this WebApplication app)
    {
        DiagnosticListener.AllListeners
            .Subscribe(new TelemetryMiddleware.MvcDiagnosticObserver());

        var otlpEndpoint = Environment.GetEnvironmentVariable("Telemetry__Endpoint")
                           ?? "http://localhost:18889";

        var resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName: "MyApi", serviceVersion: "1.0.0");

        var otlp = new Uri(otlpEndpoint);

        // ── Tracing ───────────────────────────────────────────────
        _tracerProvider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("MyApi")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = otlp;
                o.Protocol = OtlpExportProtocol.HttpProtobuf; // ← port 18889 needs this
            })
            .Build();

        // ── Metrics ───────────────────────────────────────────────
        _meterProvider = OpenTelemetry.Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("MyApi")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = otlp;
                o.Protocol = OtlpExportProtocol.HttpProtobuf; // ← same here
            })
            .Build();

        app.UseMiddleware<TelemetryMiddleware>();

        // Dispose cleanly on shutdown
        app.Lifetime.ApplicationStopping.Register(() =>
        {
            _tracerProvider?.Dispose();
            _meterProvider?.Dispose();
        });
    }
}