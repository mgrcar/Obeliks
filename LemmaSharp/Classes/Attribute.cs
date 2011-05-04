using System;
using System.Collections.Generic;
using System.Text;

namespace LemmaSharp {
    // TODO: public has to go out
    public class AttributeBase {
        public int id;
    }

    public class Attribute<ValueType> : AttributeBase {
        public ValueType val;
    }

    public abstract class AttributeSet {
    }

    public class EmptyAttributeSet : AttributeSet {
    }

}
