using Godot;
using System.IO;

public partial class SakugaNode : Node3D
{
    public virtual void SakugaPreLoop(){}
    public virtual void SakugaLoop(){}
    public virtual void SakugaPostLoop(){}

    public virtual void Serialize(BinaryWriter bw){}
    public virtual void Deserialize(BinaryReader br){}
}
