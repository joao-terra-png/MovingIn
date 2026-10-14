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
        if (vazia())
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
        if (vazia())
        {
            return false;
        }
        no = primeiro;
        primeiro = primeiro.GetNext();
        return true;
    }
    public FilaEncadeada()
    {
        primeiro = null;
        ultimo = null;
    }
}