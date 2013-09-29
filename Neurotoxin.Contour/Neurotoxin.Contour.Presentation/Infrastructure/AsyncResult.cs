using System;

namespace Neurotoxin.Contour.Presentation.Infrastructure
{
    public class AsyncResult<T>
    {
        public T Result { get; set; }
        public Exception Exception { get; set; }
        public object[] Args { get; set; }
    }
}