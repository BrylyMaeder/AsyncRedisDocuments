using System.Runtime.CompilerServices;

namespace AsyncRedisDocuments.Components
{
    public class BaseComponent
    {
        protected readonly IAsyncDocument _document;
        protected string _documentKey { get; set; }
        protected string _propertyName { get; set; }
        protected string _fullKey { get; set; }

        public BaseComponent(IAsyncDocument document, [CallerMemberName] string propertyName = "")
        {
            _documentKey = document.GetKey();
            _propertyName = propertyName;

            _fullKey = $"{_documentKey}{_propertyName}";

            _document = document;
        }
    }
}
