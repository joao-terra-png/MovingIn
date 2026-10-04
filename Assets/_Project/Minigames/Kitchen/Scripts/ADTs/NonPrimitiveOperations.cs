namespace MovingIn.Minigames.Kitchen.ADTs{
    ///<summary>
    /// operacoes nao primitivas: usam SO as operacoes publicas dos TADs
    /// </summary>
    public static class NonPrimitiveOperations
    {
        // devolve o topo da pilha SEM remove-lo
        public static bool Top<T>(this StaticStack<T> s, out T x)
        {
            if (!s.Pop(out x)) return false;   
            s.Push(x);                     
            return true;
        }

        // devolve o elemento da posicao index (0 = primeiro) SEM alterar a fila
        public static bool ElementAt<T>(this StaticQueue<T> q, int index, out T x)
        {
            x = default;
            if (index < 0 || index >= q.Quantity) return false;   

            int n = q.Quantity;
            for (int k = 0; k < n; k++)       
            {
                q.Remove(out T item);         
                if (k == index) x = item;     
                q.Insert(item);              
            }
            return true;
        }
    }
}