using UnityEngine;
using MovingIn.Minigames.Onaf;

namespace MovingIn.Core.Structures
{
    public class Arvore : MonoBehaviour
    {
        private QuartoNode raiz;

        public void Cria()
        {
            raiz = null;
        }

        public bool Vazia()
        {
            return raiz == null;
        }

        public bool EstaNaArvore(QuartoNode r, int x)
        {
            if (r == null) return false;

            if (r.GetID() == x) return true;

            if (x < r.GetID())
            {
                return EstaNaArvore(r.GetEsq(), x);
            }
            else
            {
                return EstaNaArvore(r.GetDir(), x);
            }
        }

        public bool EstaNaArvore(int x)
        {
            return EstaNaArvore(raiz, x);
        }

        public void Insere(int x, out bool ok)
        {
            if (raiz == null)
            {
                raiz = new QuartoNode(x);
                ok = true;
                return;
            }

            QuartoNode atual = raiz;

            while (true)
            {
                if (x == atual.GetID())
                {
                    ok = false;
                    return;
                }

                if (x < atual.GetID())
                {
                    if (atual.GetEsq() == null)
                    {
                        QuartoNode novo = new QuartoNode(x);
                        novo.SetPai(atual);
                        atual.SetEsq(novo);
                        ok = true;
                        return;
                    }

                    atual = atual.GetEsq();
                }
                else
                {
                    if (atual.GetDir() == null)
                    {
                        QuartoNode novo = new QuartoNode(x);
                        novo.SetPai(atual);
                        atual.SetDir(novo);
                        ok = true;
                        return;
                    }

                    atual = atual.GetDir();
                }
            }
        }

        public void Remove(int x, out bool ok)
        {
            QuartoNode alvo = BuscarNo(raiz, x);

            if (alvo == null || !alvo.IsEmpty())
            {
                ok = false;
                return;
            }

            if (alvo.GetEsq() == null)
            {
                Substituir(alvo, alvo.GetDir());
            }

            else if (alvo.GetDir() == null)
            {
                Substituir(alvo, alvo.GetEsq());
            }

            else
            {
                QuartoNode sucessor = Menor(alvo.GetDir());

                if (sucessor.GetPai() != alvo)
                {
                    Substituir(sucessor, sucessor.GetDir());

                    sucessor.SetDir(alvo.GetDir());
                    sucessor.GetDir().SetPai(sucessor);
                }

                Substituir(alvo, sucessor);

                sucessor.SetEsq(alvo.GetEsq());
                sucessor.GetEsq().SetPai(sucessor);
            }

            alvo.SetPai(null);
            alvo.SetEsq(null);
            alvo.SetDir(null);

            ok = true;
        }

        private QuartoNode BuscarNo(QuartoNode r, int x)
        {
            if (r == null || r.GetID() == x)  return r;

            if (x < r.GetID()) return BuscarNo(r.GetEsq(), x);

            return BuscarNo(r.GetDir(), x);
        }

        private QuartoNode Menor(QuartoNode r)
        {
            while (r.GetEsq() != null) r = r.GetEsq();

            return r;
        }

        private void Substituir(QuartoNode antigo, QuartoNode novo)
        {
            if (antigo.GetPai() == null)
            {
                raiz = novo;
            }
            else if (antigo == antigo.GetPai().GetEsq())
            {
                antigo.GetPai().SetEsq(novo);
            }
            else
            {
                antigo.GetPai().SetDir(novo);
            }

            if (novo != null) novo.SetPai(antigo.GetPai());
        }

        public QuartoNode GetQuarto(int id)
        {
            return BuscarNo(raiz, id);
        }

        public QuartoNode GetRaiz()
        {
            return raiz;
        }

        public bool ColocarBoneco(int id, Boneco b)
        {
            QuartoNode quarto = GetQuarto(id);

            if (quarto == null) return false;

            return quarto.SetBoneco(b);
        }

        public Boneco BuscarBoneco(int id)
        {
            QuartoNode quarto = GetQuarto(id);

            if (quarto == null) return null;

            return quarto.GetBoneco();
        }

        public void RetirarBoneco(int id)
        {
            QuartoNode quarto = GetQuarto(id);

            if (quarto != null) quarto.RemoveBoneco();
        }
    }
}