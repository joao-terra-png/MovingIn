
namespace MovingIn.Core.Structures
{
    public class FilaEncadeada
    {
        private Node primeiro;
        private Node ultimo;

        public bool Vazia()
        {
            return primeiro == null;
        }

        public void Enf(object objeto) //adiciona no final
        {
            Node novo = new Node();
            novo.SetInfo(objeto);
            novo.SetNext(null);

            if (Vazia())
            {
                primeiro = novo;
                ultimo = novo;
            }
            else
            {
                ultimo.SetNext(novo);
                ultimo = novo;
            }
        }

        public bool Desenf(Node no) //retira do comeco
        {
            if (Vazia()) return false;

            primeiro = primeiro.GetNext();
            if (primeiro == null) ultimo = null;
            return true;
        }

        public Node Primeiro()
        {
            return primeiro;
        }

        public FilaEncadeada()
        {
            primeiro = null;
            ultimo = null;
        }
    }
}
