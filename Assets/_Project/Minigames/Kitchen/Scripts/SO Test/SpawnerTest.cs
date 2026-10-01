using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    public class SpawnerTest : MonoBehaviour
    {
        [SerializeField] private ItemVisual prefabItem;
        [SerializeField] private ItemData item;

        private void Start()
        {
            ItemVisual newItem = Instantiate(prefabItem, transform.position, Quaternion.identity);
            newItem.Configure(item);
        }
    }
}