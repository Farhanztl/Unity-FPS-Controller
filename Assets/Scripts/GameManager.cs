using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    void Start()
    {
        //baraye lock kardan mouse tuye game window va invisible boodanesh
        //(baraye visible boodan az .confined estefade mikonim)
       Cursor.lockState = CursorLockMode.Locked;
    }

   
}
