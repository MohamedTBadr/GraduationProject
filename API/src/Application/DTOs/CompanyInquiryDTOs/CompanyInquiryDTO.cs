using Application.DTOs.CategoryDTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CompanyInquiryDTOs
{
    public class CompanyInquiryDto
    {
        public Guid Id { get; set; }

        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public EventTypesDTOs.EventTypeResponseDto EventType { get; set; }

        public DateTime ExpectedDate { get; set; }

        public int EstimatedAttendees { get; set; }

        public decimal ApproximateBudget { get; set; }

        public string AdditionalRequirements { get; set; }

        public string Status { get; set; } = "Pending";
    
    }

    public class UpdateCompanyInquiryDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public Guid EventTypeId { get; set; }

        public DateTime ExpectedDate { get; set; }

        public int EstimatedAttendees { get; set; }

        public decimal ApproximateBudget { get; set; }

        public string AdditionalRequirements { get; set; }

        public string Status { get; set; } = "Pending";
    }

    public class CreateCompanyInquiryDto
    {

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public Guid EventTypeId { get; set; }

        public DateTime ExpectedDate { get; set; }

        public int EstimatedAttendees { get; set; }

        public decimal ApproximateBudget { get; set; }

        public string AdditionalRequirements { get; set; }

        public string Status { get; set; } = "Pending";

    }
}
