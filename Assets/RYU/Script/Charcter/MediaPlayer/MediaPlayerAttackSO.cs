
namespace RYU.Script.Charcter.MediaPlayer
{
    public class MediaPlayerAttackSO : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            if (unit is not MediaPlayerCharacter media)
                return;

            _ = media.SelectEnemyAsync();
        }
    }
}
