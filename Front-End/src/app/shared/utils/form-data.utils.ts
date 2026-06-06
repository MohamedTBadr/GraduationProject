/**
 * Appends files for ASP.NET Core List<IFormFile> model binding.
 * Uses repeated field names (e.g. ServiceImages, ServiceImages, …).
 */
export function appendFormFileList(formData: FormData, fieldName: string, files: File[]): void {
  files.forEach(file => formData.append(fieldName, file, file.name));
}

/** Appends a single optional file with the correct filename for multipart binding. */
export function appendFormFile(formData: FormData, fieldName: string, file: File | null | undefined): void {
  if (file) formData.append(fieldName, file, file.name);
}
