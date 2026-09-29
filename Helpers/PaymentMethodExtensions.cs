using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Helpers
{
    public static class PaymentMethodExtensions
    {
        public static string ToLabel(this PaymentMethod method)
        {
            var member = typeof(PaymentMethod).GetMember(method.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? method.ToString();
        }
    }
}
