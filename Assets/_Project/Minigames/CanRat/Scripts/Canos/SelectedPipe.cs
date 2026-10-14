using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedPipe : MonoBehaviour
{
    // Start is called before the first frame update
    //Indica se o objeto está ou não selecionado
    private bool IsSelected;

    public bool GetIsSelected()
    {
        return IsSelected;
    }
    void Start()
    {
        IsSelected = false;
    }

    // Update is called once per frame
    //Marca o objeto selecionado (e desmarca)
    void MarkSelected(){
        if(IsSelected == true)
            gameObject.transform.position += -0.2f * Vector3.forward;
        else
            gameObject.transform.position += 0.2f * Vector3.forward;
    }


    public void SelectPipe()
    {
        IsSelected = !IsSelected;
        MarkSelected();
    }
}
