using Godot;
using System;

public class ObstacleBody : KinematicBody
{
        
    public override void _Ready()
    {
		GD.Randomize();
		RotationDegrees += new Vector3(0, 0, (float)GD.RandRange(0,360));
	}

	public override void _Process(float delta)
	{
	
		Translation += new Vector3(0, 0, -0.15f)*RangeLerp((GetParent<Main>().time),0,60,1,2.5f);
		
	}
	public float RangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}
}
