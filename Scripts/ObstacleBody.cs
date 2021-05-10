using Godot;

public class ObstacleBody : KinematicBody
{	
	Vector3 velocity;
	public override void _Ready()
    {

		GD.Randomize();
		float time = GetParent<Main>().time;
		float trackLenght = GetParent<Main>().trackLenght;
		velocity = new Vector3(
			0, 
			0, 
			-0.08f * Main.RangeLerp(time, 0, trackLenght, 3f, 5f)
			);
		
		GetNode<CollisionShape>("1").RotationDegrees += new Vector3(
			0, 
			0, 
			(float)GD.RandRange(0,360)
			);

		RotationDegrees += new Vector3(
			0,
			0, 
			(float)GD.RandRange(0, 360)
			);

		float obstacleOffset = 0;

		GetNode<CollisionShape>("1").Translation += new Vector3( 
			(float)GD.RandRange(-obstacleOffset, obstacleOffset),
			(float)GD.RandRange(-obstacleOffset, obstacleOffset),
			0
			);
		
	}	
	public override void _Process(float delta)
	{
		Translation += velocity*delta*100 ;
	}
}
