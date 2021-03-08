using Godot;
using System;

public class ObstacleBody : KinematicBody
{	
	Vector3 velocity;
	public override void _Ready()
    {
		GD.Randomize();

		float time = GetParent<Main>().time;

		float offset = GetParent<Main>().RangeLerp(time, 0, 60, 1, 2.5f);

		velocity = new Vector3(0, 0, -0.15f) * offset;

		RotationDegrees += new Vector3(0, 0, (float)GD.RandRange(0,360));

		float obstacleOffset = 1-offset * 0.25f;

		GetNode<CollisionShape>("1").Translation += new Vector3( (float)GD.RandRange(-offset, offset),
																 (float)GD.RandRange(-offset, offset),
																 0);
		GetNode<CollisionShape>("2").Translation += new Vector3( (float)GD.RandRange(-offset, offset),
																 (float)GD.RandRange(-offset, offset),
																 0);
	}	
	public override void _Process(float delta)
	{
		Translation += velocity ;
	}
}
