using Godot;
using System;

public class Main : Spatial
{
	public float time = 0;
	float whileMoving = 0;
	Vector2 moveDirection = new Vector2(0, 0);
	float moveSpeed = 0.02f;

	Area head;

	PackedScene obstacleScene;
	KinematicBody obstacle;
	
	SpatialMaterial material;

	AudioEffectSpectrumAnalyzerInstance spectrum;

	float high;
	float mid;
	float low;


	public override void _Ready()
	{
		head = GetNode<Area>("Head");
		material = GD.Load<SpatialMaterial>("res://Materials/1.tres");
		obstacleScene = GD.Load<PackedScene>("res://Scenes/ObstacleBody.tscn");
		
		
	}


	public override void _Process(float delta)
	{
		
		moveDirection = new Vector2
			(
				-Convert.ToInt32(Input.IsActionPressed("ui_right")) + Convert.ToInt32(Input.IsActionPressed("ui_left")),
				-Convert.ToInt32(Input.IsActionPressed("ui_down"))  + Convert.ToInt32(Input.IsActionPressed("ui_up"))
			);

		
		moveDirection = moveDirection.Normalized();

		moveDirection = new Vector2
		(
			moveDirection.x, 	
			moveDirection.y
		)*0.86f;

		if (moveDirection != new Vector2(0, 0))
		{
			whileMoving += delta;
			moveSpeed = RangeLerp(whileMoving, 0, 0.2f, 0.02f, 0.025f);
			head.Translation = new Vector3
			(
				Mathf.MoveToward(head.Translation.x, moveDirection.x, moveSpeed),
				Mathf.MoveToward(head.Translation.y, moveDirection.y, moveSpeed),
				0
			);
			
		} else whileMoving -=delta;

		whileMoving = Mathf.Clamp(whileMoving, 0, 0.2f);
		
		spectrum = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(0, 0);

		high = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(7000, 16000).Length()),-30, 10, 0, 255);
		mid = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(500, 8000).Length()), -30,10,0,255);
		low = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(30, 100).Length()), -30,10,0,255);
		
		material.AlbedoColor = Color.Color8
		(
			(byte)high, 
			(byte)mid, 
			(byte)low, 
			255
		);

		time += delta;

	}
	public void _on_Timer_timeout()
	{
		Spatial spatial = GetNode<Spatial>("Spatial/Tunnel/Spatial");
		obstacle = (KinematicBody)obstacleScene.Instance();
		AddChild(obstacle);
		obstacle.Translation = new Vector3(0,0,26);
	}
	public float RangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}

	public void _on_ObstacleKiller_body_entered(Node body)
	{
		body.QueueFree();
	}
	public void _on_Head_body_entered(Node body)
	{
		GetTree().ReloadCurrentScene();
	}
	public void _on_ItemList_item_selected(int index)
	{

		var itemList = GetNode<ItemList>("Control/ItemList");
		itemList.Hide();

		GetNode<Timer>("Timer").Start();
		GetNode<AudioStreamPlayer>("AudioStreamPlayer3D").Stream = GD.Load<AudioStream>("res://Audio/" + itemList.GetItemText(index) + ".ogg");
		GetNode<AudioStreamPlayer>("AudioStreamPlayer3D").Play();
	}
}	
