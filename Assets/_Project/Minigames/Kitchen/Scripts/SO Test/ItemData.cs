using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    public enum ItemType { Plate, Utensil, Other }

    [CreateAssetMenu(fileName = "NewItem", menuName = "MovingIn/Kitchen/Item")]
    public class ItemData : ScriptableObject
    {
        public string exhibitName;
        public Sprite sprite;
        public Color colorPlaceholder = Color.white;
        public ItemType type;
        [Range(1, 3)] public int size = 1; 
    }
}
