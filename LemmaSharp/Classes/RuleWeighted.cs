/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
