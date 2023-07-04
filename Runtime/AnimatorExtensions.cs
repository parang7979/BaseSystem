using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Parang
{
    static public class AnimatorExtensions
    {
        static async public UniTask WaitAnimatorEnd(this Animator animator, string name)
        {
            if (animator == null) return;
            await animator.WaitAnimatorStart(name);
            if (animator == null) return;
            var info = animator.GetCurrentAnimatorStateInfo(0);
            await UniTask.Delay((int)(info.length * 1000));
        }

        static async public UniTask WaitAnimatorStart(this Animator animator, string name)
        {
            while (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(name))
                await UniTask.Yield();
        }
    }
}
