using Godot;
using System;

public class Main : Spatial
{
	public float time = 0;
	float moveSpeed = 0.02f;
	float high;
	float mid;
	float low;

	int close = 0;

	float health = 100;

	float progress = 0;

	float trackLenght;
	ProgressBar healthBar;
	ProgressBar progressBar;
	Area head;
	PackedScene obstacleScene;
	KinematicBody obstacle;
	SpatialMaterial material;
	AudioEffectSpectrumAnalyzerInstance spectrum;
	Vector2 moveDirection = new Vector2(0, 0);

	public override void _Ready()
	{
		head = GetNode<Area>("Head");
		healthBar = GetNode<ProgressBar>("Control/Health");
		progressBar = GetNode<ProgressBar>("Control/Progress");


		material = GD.Load<SpatialMaterial>("res://Materials/1.tres");
		obstacleScene = GD.Load<PackedScene>("res://Scenes/ObstacleBody.tscn");
		
		GetNode<FileDialog>("Control/FileDialog").Show();
		
		GetNode<FileDialog>("Control/FileDialog").Invalidate();
	}

	public override void _PhysicsProcess(float delta)
	{
		moveDirection = new Vector2(-Convert.ToInt32(Input.IsActionPressed("ui_right")) + Convert.ToInt32(Input.IsActionPressed("ui_left")),
									-Convert.ToInt32(Input.IsActionPressed("ui_down")) + Convert.ToInt32(Input.IsActionPressed("ui_up"))
									);

		

		if (moveDirection != new Vector2(0, 0))
		{
			moveDirection = moveDirection.Normalized();

			moveDirection = new Vector2(
										moveDirection.x,
										moveDirection.y
										) * moveSpeed;		

			var a = new Vector2(head.Translation.x, head.Translation.y) + moveDirection;
			if (a.DistanceTo(new Vector2(0, 0)) < 0.9)
			{
				head.Translation += new Vector3( moveDirection.x,
												 moveDirection.y,
												 0);
			}
			else
			{
				health -= 0.5f;
				
				if (health < 0) GetTree().ReloadCurrentScene();
			}
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
		progressBar.Value = time;
		healthBar.Value = Mathf.MoveToward((float)healthBar.Value, health, 0.5f);
		if (Input.IsActionJustPressed("ui_cancel")) GetTree().Quit();
	}
	void _on_Timer_timeout()
	{
		Spatial spatial = GetNode<Spatial>("Spatial/Tunnel/Spatial");
		obstacle = (KinematicBody)obstacleScene.Instance();
		AddChild(obstacle);
		obstacle.Translation = new Vector3(0, 0, 26);
		

	}
	public float RangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}
	void _on_ObstacleKiller_body_entered(Node body)
	{
		body.QueueFree();
	}
	void _on_Head_body_entered(Node body)
	{
		health -= 25;
		
		if (health < 0) GetTree().ReloadCurrentScene();
	}
	void _on_FileDialog_file_selected(string path){
		close -= 1;

		GetNode<Timer>("Timer").Start();

		AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer3D");

		audioPlayer.Stream = GD.Load<AudioStream>(path);

		progressBar.MaxValue = audioPlayer.Stream.GetLength();

		audioPlayer.Play();
	}
	void _on_FileDialog_hide()
	{
		close += 1;
		if (close == 2) GetTree().ReloadCurrentScene();
	}
}
