using Godot;

namespace ChronoShift;

/// <summary>
/// Closed-form two-bone IK for an arm: given a shoulder, an elbow and a wrist,
/// it bends the pair so the wrist lands on a target. Used to put the support
/// hand on a weapon's foregrip, which no clip in the character pack provides.
///
/// Everything is solved in the skeleton's own space and written back as local
/// bone poses, so it composes with whatever the animation did that frame.
/// </summary>
public static class ArmIk
{
    /// <summary>
    /// Bends <paramref name="upper"/> and <paramref name="lower"/> so the tip of
    /// the chain reaches <paramref name="targetGlobal"/>. <paramref name="poleGlobal"/>
    /// biases which way the elbow points. <paramref name="weight"/> blends the
    /// result against the pose already in place.
    /// </summary>
    public static void Solve(
        Skeleton3D skeleton,
        int upper,
        int lower,
        int tip,
        Vector3 targetGlobal,
        Vector3 poleGlobal,
        float weight)
    {
        if (upper < 0 || lower < 0 || tip < 0 || weight <= 0.001f)
        {
            return;
        }

        Transform3D skeletonInverse = skeleton.GlobalTransform.AffineInverse();
        Vector3 target = skeletonInverse * targetGlobal;
        Vector3 pole = skeletonInverse * poleGlobal;

        Transform3D upperPose = skeleton.GetBoneGlobalPose(upper);
        Transform3D lowerPose = skeleton.GetBoneGlobalPose(lower);
        Transform3D tipPose = skeleton.GetBoneGlobalPose(tip);

        Vector3 shoulder = upperPose.Origin;
        Vector3 elbow = lowerPose.Origin;
        Vector3 wrist = tipPose.Origin;

        float upperLength = (elbow - shoulder).Length();
        float lowerLength = (wrist - elbow).Length();
        if (upperLength < 0.0001f || lowerLength < 0.0001f)
        {
            return;
        }

        Vector3 toTarget = target - shoulder;
        float reach = toTarget.Length();
        if (reach < 0.0001f)
        {
            return;
        }

        // Keep the target inside what the arm can actually cover, just shy of
        // locked-straight so the elbow angle stays defined.
        float minReach = Mathf.Abs(upperLength - lowerLength) + 0.001f;
        float maxReach = upperLength + lowerLength - 0.001f;
        reach = Mathf.Clamp(reach, minReach, maxReach);

        Vector3 direction = toTarget.Normalized();

        // The elbow swings in the plane spanned by the target direction and the
        // pole; fall back to the current elbow when the pole is degenerate.
        Vector3 poleDirection = pole - shoulder;
        poleDirection -= direction * poleDirection.Dot(direction);
        if (poleDirection.LengthSquared() < 0.000001f)
        {
            poleDirection = elbow - shoulder;
            poleDirection -= direction * poleDirection.Dot(direction);
        }

        if (poleDirection.LengthSquared() < 0.000001f)
        {
            return;
        }

        poleDirection = poleDirection.Normalized();

        float cosShoulder = Mathf.Clamp(
            (upperLength * upperLength + reach * reach - lowerLength * lowerLength) / (2.0f * upperLength * reach),
            -1.0f,
            1.0f);
        float shoulderAngle = Mathf.Acos(cosShoulder);

        Vector3 newElbow = shoulder
                           + (direction * Mathf.Cos(shoulderAngle) + poleDirection * Mathf.Sin(shoulderAngle))
                           * upperLength;

        PointBone(skeleton, upper, lower, newElbow - shoulder, weight);
        PointBone(skeleton, lower, tip, target - newElbow, weight);
    }

    /// <summary>
    /// Turns a bone to a wanted orientation rather than just a direction — used
    /// for the support hand, where the palm has to face the weapon and not merely
    /// end up in the right place.
    /// </summary>
    public static void OrientBone(Skeleton3D skeleton, int bone, Basis wantedGlobal, float weight)
    {
        if (bone < 0 || weight <= 0.001f)
        {
            return;
        }

        int parent = skeleton.GetBoneParent(bone);
        Basis parentBasis = parent >= 0 ? skeleton.GetBoneGlobalPose(parent).Basis : Basis.Identity;
        Basis wantedLocal = parentBasis.Inverse() * wantedGlobal.Orthonormalized();

        Quaternion current = skeleton.GetBonePoseRotation(bone);
        Quaternion blended = current.Slerp(wantedLocal.GetRotationQuaternion(), Mathf.Clamp(weight, 0.0f, 1.0f));
        skeleton.SetBonePoseRotation(bone, blended);
    }

    /// <summary>
    /// Rotates <paramref name="bone"/> so its child ends up along
    /// <paramref name="directionGlobal"/>, keeping the twist the bone already had.
    /// </summary>
    private static void PointBone(
        Skeleton3D skeleton,
        int bone,
        int child,
        Vector3 directionGlobal,
        float weight)
    {
        if (directionGlobal.LengthSquared() < 0.000001f)
        {
            return;
        }

        Vector3 childOffset = skeleton.GetBoneRest(child).Origin;
        if (childOffset.LengthSquared() < 0.000001f)
        {
            return;
        }

        Transform3D bonePose = skeleton.GetBoneGlobalPose(bone);
        Vector3 currentDirection = (bonePose.Basis * childOffset).Normalized();
        Vector3 wantedDirection = directionGlobal.Normalized();

        var delta = new Quaternion(currentDirection, wantedDirection);
        delta = Quaternion.Identity.Slerp(delta, Mathf.Clamp(weight, 0.0f, 1.0f));

        // The delta is in skeleton space; move it into the bone's parent space
        // before it can be written back as a local pose.
        int parent = skeleton.GetBoneParent(bone);
        Basis parentBasis = parent >= 0 ? skeleton.GetBoneGlobalPose(parent).Basis : Basis.Identity;
        Basis local = parentBasis.Inverse() * new Basis(delta) * bonePose.Basis;

        skeleton.SetBonePoseRotation(bone, local.Orthonormalized().GetRotationQuaternion());
    }
}
