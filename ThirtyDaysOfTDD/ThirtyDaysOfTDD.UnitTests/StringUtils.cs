using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThirtyDaysOfTDD.UnitTests
{
    public class StringUtils
    {
        public int FindNumberOfOccurences(string sentenceToScan, string characterToScanFor)
        {
            if ((characterToScanFor??"").Length>1)
            {
                throw new ArgumentException(""); 
            }
            return (sentenceToScan??"").Where(c => c == (characterToScanFor ?? "").FirstOrDefault()).Count();
            //throw new NotImplementedException();
        }
    }
}
