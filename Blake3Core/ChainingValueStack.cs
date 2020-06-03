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

        Flag _defaultFlag;
        uint[] _key;
        int _depth = 0;
        ChainingValue[] _stack = new ChainingValue[MaxDepth + 1];

        public ChainingValueStack(Flag defaultFlag, ReadOnlyMemory<uint> key)
        {
            _defaultFlag = defaultFlag;
            _key = key.ToArray();
        }

        static int CountOneBits(ulong value)
        {
            unchecked
            {
                int count = 0;
                while (value != 0)
                {
                    value &= (value - 1);
                    count++;
                }
                return count;
            }
        }

        ChainingValue ComputeParentChainingValue(ReadOnlySpan<byte> block)
        {
            //$ TODO compute chaining value using _defaultFlag | Flag.Parent and _key
            return default(ChainingValue);
        }

        // As described in hasher_push_cv() below, we do "lazy merging", delaying
        // merges until right before the next CV is about to be added. This is
        // different from the reference implementation. Another difference is that we
        // aren't always merging 1 chunk at a time. Instead, each CV might represent
        // any power-of-two number of chunks, as long as the smaller-above-larger stack
        // order is maintained. Instead of the "count the trailing 0-bits" algorithm
        // described in the spec, we use a "count the total number of 1-bits" variant
        // that doesn't require us to retain the subtree size of the CV on top of the
        // stack. The principle is the same: each CV that should remain in the stack is
        // represented by a 1-bit in the total number of chunks (or bytes) so far.
        void MergeStack(ulong chunkCount)
        {
            var postMergeStackDepth = CountOneBits(chunkCount);
            while (_depth > postMergeStackDepth)
            {
                var blockBytes = _stack.AsSpan(_depth - 2).AsBytes();
                _stack[_depth - 2] = ComputeParentChainingValue(blockBytes);               
                _depth--;
            }
        }

        // In reference_impl.rs, we merge the new CV with existing CVs from the stack
        // before pushing it. We can do that because we know more input is coming, so
        // we know none of the merges are root.
        //
        // This setting is different. We want to feed as much input as possible to
        // compress_subtree_wide(), without setting aside anything for the chunk_state.
        // If the user gives us 64 KiB, we want to parallelize over all 64 KiB at once
        // as a single subtree, if at all possible.
        //
        // This leads to two problems:
        // 1) This 64 KiB input might be the only call that ever gets made to update.
        //    In this case, the root node of the 64 KiB subtree would be the root node
        //    of the whole tree, and it would need to be ROOT finalized. We can't
        //    compress it until we know.
        // 2) This 64 KiB input might complete a larger tree, whose root node is
        //    similarly going to be the the root of the whole tree. For example, maybe
        //    we have 196 KiB (that is, 128 + 64) hashed so far. We can't compress the
        //    node at the root of the 256 KiB subtree until we know how to finalize it.
        //
        // The second problem is solved with "lazy merging". That is, when we're about
        // to add a CV to the stack, we don't merge it with anything first, as the
        // reference impl does. Instead we do merges using the *previous* CV that was
        // added, which is sitting on top of the stack, and we put the new CV
        // (unmerged) on top of the stack afterwards. This guarantees that we never
        // merge the root node until finalize().
        //
        // Solving the first problem requires an additional tool,
        // compress_subtree_to_parent_node(). That function always returns the top
        // *two* chaining values of the subtree it's compressing. We then do lazy
        // merging with each of them separately, so that the second CV will always
        // remain unmerged. (That also helps us support extendable output when we're
        // hashing an input all-at-once.)
        public void Push(ref ChainingValue cv, ulong chunkCount)
        {
            MergeStack(chunkCount);
            _stack[_depth++] = cv;
        }
    }
}
