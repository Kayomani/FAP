using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookLibrary.Domain.Test
{
    internal static class TestHelper
    {
        public static string CreateString(char character, int count)
        {
            StringBuilder builder = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                builder.Append(character);
            }
            return builder.ToString();
        }
    }
}
