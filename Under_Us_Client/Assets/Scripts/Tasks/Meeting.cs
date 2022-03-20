using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Meeting : MonoBehaviour
{
    public void Voted()
    {
        Debug.Log("Nutton selected : " + EventSystem.current.currentSelectedGameObject.transform.parent.name);
    }
}
