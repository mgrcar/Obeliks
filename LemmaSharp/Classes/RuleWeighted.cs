/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    
 *    The MIT License (MIT)
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    Permission is hereby granted, free of charge, to any person obtaining 
 *    a copy of this software and associated documentation files (the 
 *    "Software"), to deal in the Software without restriction, including 
 *    without limitation the rights to use, copy, modify, merge, publish, 
 *    distribute, sublicense, and/or sell copies of the Software, and to 
 *    permit persons to whom the Software is furnished to do so, subject to 
 *    the following conditions:
 *
 *    The above copyright notice and this permission notice shall be 
 *    included in all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
 *    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
 *    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
 *    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 *    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace LemmaSharp {
    [Serializable()]
    //TODO_30 change into non public
    public class RuleWeighted: IComparable<RuleWeighted>{
        #region Private Variables

        private LemmaRule lrRule;
        private double dWeight;

        #endregion

        #region Constructor(s) & Destructor(s)

        public RuleWeighted(LemmaRule lrRule, double dWeight) {
            this.lrRule = lrRule;
            this.dWeight = dWeight;
        }

        #endregion

        #region Public Properties

        public LemmaRule Rule {
            get { return lrRule; }
        }
        public double Weight {
            get { return dWeight; }
        }

        #endregion

        #region Essential Class Functions (comparing objects, eg.: for sorting)

        public int CompareTo(RuleWeighted rl) {
            if (this.dWeight < rl.dWeight) return 1;
            if (this.dWeight > rl.dWeight) return -1;
            if (this.lrRule.Id < rl.lrRule.Id) return 1;
            if (this.lrRule.Id > rl.lrRule.Id) return -1;
            return 0;
        }

        #endregion

        #region Output & Serialization Functions

        public override string ToString() {
            return lrRule.ToString() + dWeight.ToString("(0.00%)");
        }

        public string ToString(double dAllInstances) {
            return lrRule.ToString() + (dWeight * dAllInstances).ToString("(num:#,0)");
        }

        #endregion
    }
}
