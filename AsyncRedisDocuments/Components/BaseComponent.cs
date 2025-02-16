
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AsyncRedisDocuments.Components
{
    public class BaseComponent
    {
        protected readonly IAsyncDocument _document;
        protected string _documentKey { get; set; }
        protected string _propertyName { get; set; }
        protected string _fullKey { get; set; }

        public BaseComponent(IAsyncDocument document = null, [CallerMemberName] string propertyName = "")
        {
            if (document == null)
            {
                _documentKey = "global_settings";
            }
            else
            {
                _documentKey = document.GetKey();
            }

            _document = document;

            _propertyName = propertyName;
            _fullKey = $"{_documentKey}:{_propertyName}";
        }
    }
}
