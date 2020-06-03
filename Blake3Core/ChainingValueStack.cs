using System;
using System.Collections.Generic;
using System.Text;

namespace Blake3Core
{
    class ChainingValueStack
    {
        // The stack size is MAX_DEPTH + 1 because we do lazy merging. For example,
        // with 7 chunks, we have 3 entries in the stack. Adding an 8th chunk
        // requires a 4th entry, rather than merging everything down to 1, because we
        // don't know whether more input is coming. This is different from how the
        // reference implementation does things.
        const int MaxDepth = 54;

        int _depth = 0;
        ChainingValue[] _stack = new ChainingValue[MaxDepth + 1];

        public void Push(ref ChainingValue cv)
        {
            _stack[_depth++] = cv;
        }
    }
}
