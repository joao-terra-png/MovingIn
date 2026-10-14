using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GerenciadorPosicionamento : MonoBehaviour
{
    [SerializeField] private LojaAliados loja;
    [SerializeField] private GerenciadorOndas ondas;
    [SerializeField] private GerenciadorEnergia energia;
    [SerializeField] private LayerMask camadaSlots;

    private readonly List<Aliado> aliados = new List<Aliado>();
    private Camera cam;
    private bool bloqueado;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (bloqueado || loja.AliadoSelecionado == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            loja.Cancelar();
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.transform.position.z;
        Vector2 ponto = cam.ScreenToWorldPoint(mouse);

        Collider2D col = Physics2D.OverlapPoint(ponto, camadaSlots);
        if (col == null) return;

        SlotAliado slot = col.GetComponent<SlotAliado>();
        if (slot != null) TentarPosicionar(slot);
    }

    private bool TentarPosicionar(SlotAliado slot)
    {
        if (slot.Ocupado) return false;

        AliadoData dados = loja.AliadoSelecionado;
        if (dados.prefab == null)
        {
            Debug.LogError("O AliadoData '" + dados.nome + "' está sem Prefab.");
            loja.Cancelar();
            return false;
        }

        if (!loja.TentarComprar(dados))
        {
            loja.Cancelar();
            return false;
        }

        Aliado aliado = Instantiate(dados.prefab, slot.transform.position, Quaternion.identity);
        aliado.Inicializar(dados, ondas, energia);

        slot.Ocupar();
        aliados.Add(aliado);
        loja.Cancelar();
        return true;
    }

    public void Encerrar()
    {
        bloqueado = true;
        foreach (Aliado aliado in aliados)
            if (aliado != null) aliado.Desativar();
    }
}
