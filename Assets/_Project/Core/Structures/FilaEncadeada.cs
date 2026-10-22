using UnityEngine;

public class FilaEncadeada : MonoBehaviour
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
    public bool Desenf(object objeto) //retira do comeco
    {
        if (Vazia())
        {
            return false;
        }
        objeto = primeiro;
        primeiro = primeiro.GetNext();
        return true;
    }
    public FilaEncadeada()
    {
        primeiro = null;
        ultimo = null;
    }
}