using Monica.Common.Utils;

namespace Monica.Common.Pocos
{
    public abstract class SerializablePoco 
    {

        public virtual string Serialize()
        {
            return SerializerHelper.Serialize(this);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
