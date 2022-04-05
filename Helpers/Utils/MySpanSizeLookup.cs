﻿using AndroidX.RecyclerView.Widget;

namespace WoWonder.Helpers.Utils
{
    public class MySpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        private readonly int SpanPos, SpanCnt1, SpanCnt2;
        public MySpanSizeLookup(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {

            return position % SpanPos == 0 ? SpanCnt2 : SpanCnt1;
        }
    }

    public class MySpanSizeLookup2 : GridLayoutManager.SpanSizeLookup
    {
        private readonly int SpanPos, SpanCnt1, SpanCnt2;
        public MySpanSizeLookup2(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {
            if (position >= SpanPos)
            {
                return SpanCnt1;
            }
            else
            {
                return SpanCnt2;
            } 
        }
    }

    public class MySpanSizeLookup3 : GridLayoutManager.SpanSizeLookup
    {
        private readonly int SpanPos, SpanCnt1, SpanCnt2;
        public MySpanSizeLookup3(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {
            if (position >= SpanPos)
            {
                return SpanCnt1;
            }
            else
            {
                return SpanCnt2;
            }

        }
    }
}