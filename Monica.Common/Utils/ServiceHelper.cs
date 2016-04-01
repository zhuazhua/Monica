using System;
using System.Linq;

namespace Monica.Common.Utils
{
    public static class ServiceHelper
    {
        public static string GenerateUid()
        {
            var i = Guid.NewGuid().ToByteArray().Aggregate<byte, long>(1, (current, b) => current*((int) b + 1));
            return $"{i - DateTime.Now.Ticks:x}";
        }
    }
}
