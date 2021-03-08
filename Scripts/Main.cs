using Godot;
using System;

public class Main : Spatial
{
	public float time = 0;
	float whileMoving = 0;
	float moveSpeed = 0.02f;
	float high;
	float mid;
	float low;

	int close = 0;

	Area head;
	PackedScene obstacleScene;
	KinematicBody obstacle;
	SpatialMaterial material;
	AudioEffectSpectrumAnalyzerInstance spectrum;
	Vector2 moveDirection = new Vector2(0, 0);

	public override void _Ready()
	{
		head = GetNode<Area>("Head");
		material = GD.Load<SpatialMaterial>("res://Materials/1.tres");
		obstacleScene = GD.Load<PackedScene>("res://Scenes/ObstacleBody.tscn");
		GetNode<FileDialog>("Control/FileDialog").Show();
		GetNode<FileDialog>("Control/FileDialog").Invalidate();
	}

	public override void _Process(float delta)
	{
		moveDirection = new Vector2(
									-Convert.ToInt32(Input.IsActionPressed("ui_right")) + Convert.ToInt32(Input.IsActionPressed("ui_left")),
									-Convert.ToInt32(Input.IsActionPressed("ui_down")) + Convert.ToInt32(Input.IsActionPressed("ui_up"))
									);


		moveDirection = moveDirection.Normalized();

		moveDirection = new Vector2
		(
			moveDirection.x,
			moveDirection.y
		) * moveSpeed;

		if (moveDirection != new Vector2(0, 0))
		{
			moveSpeed = RangeLerp(whileMoving, 0, 0.2f, 0.02f, 0.025f);

			var a = new Vector2(head.Translation.x, head.Translation.y) + moveDirection;
			if (a.DistanceTo(new Vector2(0, 0)) < 0.8)
			{
				head.Translation += new Vector3(
												moveDirection.x,
												moveDirection.y,
												0
												);
			}
			else GetTree().ReloadCurrentScene();
		}

		spectrum = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(0, 0);

		high = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(7000, 16000).Length()), -30, 10, 0, 255);
		mid = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(500, 8000).Length()), -30, 10, 0, 255);
		low = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(30, 100).Length()), -30, 10, 0, 255);

		material.AlbedoColor = Color.Color8
		(
			(byte)high,
			(byte)mid,
			(byte)low,
			255
		);
		time += delta;
	}

	void _on_Timer_timeout()
	{
		Spatial spatial = GetNode<Spatial>("Spatial/Tunnel/Spatial");
		obstacle = (KinematicBody)obstacleScene.Instance();
		AddChild(obstacle);
		obstacle.Translation = new Vector3(0, 0, 26);
	}
	float RangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}

	void _on_ObstacleKiller_body_entered(Node body)
	{
		body.QueueFree();
	}
	void _on_Head_body_entered(Node body)
	{
		GetTree().ReloadCurrentScene();
	}
	
	void _on_FileDialog_file_selected(string path){
		close -= 1;
		GetNode<Timer>("Timer").Start();
		GetNode<AudioStreamPlayer>("AudioStreamPlayer3D").Stream = GD.Load<AudioStream>(path);
		GetNode<AudioStreamPlayer>("AudioStreamPlayer3D").Play();
	}
	void _on_FileDialog_hide()
	{
		close += 1;
		if (close == 2) GetTree().ReloadCurrentScene();
	}

}
