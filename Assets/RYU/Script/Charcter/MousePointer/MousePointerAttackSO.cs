
namespace RYU.Script.Charcter.MousePointer
{
    public class MousePointerAttackSO : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            if (unit is MousePointerCharacter pointer)
                pointer.BeginDrag();
        }
    }
}
