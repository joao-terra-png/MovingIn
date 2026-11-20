using MovingIn.Minigames.Onaf;
namespace MovingIn.Core.Structures
{
    public class QuartoNode
    {
        private int ID;

        private QuartoNode pai;
        private QuartoNode esq;
        private QuartoNode dir;

        private Boneco boneco;

        public QuartoNode(int id)
        {
            ID = id;
            pai = null;
            esq = null;
            dir = null;
            boneco = null;
        }

        public int GetID()
        {
            return ID;
        }

        public void SetID(int id)
        {
            ID = id;
        }

        public QuartoNode GetPai()
        {
            return pai;
        }

        public void SetPai(QuartoNode novoPai)
        {
            pai = novoPai;
        }

        public QuartoNode GetEsq()
        {
            return esq;
        }

        public void SetEsq(QuartoNode novoEsq)
        {
            esq = novoEsq;
        }

        public QuartoNode GetDir()
        {
            return dir;
        }

        public void SetDir(QuartoNode novoDir)
        {
            dir = novoDir;
        }

        public bool SetBoneco(Boneco b)
        {
            if (boneco != null || b == null)
                return false;

            boneco = b;
            return true;
        }

        public Boneco GetBoneco()
        {
            return boneco;
        }

        public void RemoveBoneco()
        {
            boneco = null;
        }

        public bool IsEmpty()
        {
            return boneco == null;
        }
    }
}