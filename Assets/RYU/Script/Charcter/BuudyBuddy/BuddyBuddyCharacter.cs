using UnityEngine;

namespace RYU.Script.Charcter.BuudyBuddy
{
    public class BuddyBuddyCharacter : AbstractUnit
    {
        [SerializeField] private Vector2 boxSize;
        [SerializeField] private LayerMask whatIsTeam;
        public void GetNearCharacter()
        {
            var cols = Physics2D.OverlapBoxAll(transform.position, boxSize, 0, whatIsTeam);
            foreach (var col in cols)
            {
            
            }
        }
    }
}
