using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcLoggingDemo.Models
{
    /// <summary>
    /// Credits : This class is from the NerdDinner sample application
    /// </summary>
    /// <see cref="http://www.nerddinner.com/"/>
    public class RuleViolation
    {

        public string ErrorMessage { get; private set; }
        public string PropertyName { get; private set; }

        public RuleViolation(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public RuleViolation(string errorMessage, string propertyName)
        {
            ErrorMessage = errorMessage;
            PropertyName = propertyName;
        }
    }
}