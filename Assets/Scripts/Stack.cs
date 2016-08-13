using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stack : MonoBehaviour {

	AudioSource tapSound;//required for playing sound on tap

	public Text scoreText;

	public Color32[] gameColors = new Color32[4];

	public Material stackMat;//custom shader

	private const float BOUNDS_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0F;
	private const float ERROR_MARGIN = 0.1f;
	private const float STACK_BOUNDS_GAIN = 0.25f;
	private const int COMBO_START_GAIN = 2;

	private GameObject[] stack;
	public GameObject endPanel;

	private int stackIndex;
	private int scoreCount=0;
	private int combo;

	private float tileTransition=0f;
	private float tileSpeed = 2.5f;
	private float secondaryPosition;

	private bool isMovingOnX=true;
	private bool gameOver = false;

	private Vector2 stackBounds = new Vector2 (BOUNDS_SIZE, BOUNDS_SIZE);

	private Vector3 desiredPosition;
	private Vector3 lastTilePostion;

	// Use this for initialization
	void Start () {
		




		tapSound = GetComponent<AudioSource>();

		stack = new GameObject[transform.childCount];

		for (int i = 0; i < transform.childCount; i++) {
			stack [i] = transform.GetChild (i).gameObject;
			ColorMesh(stack [i].GetComponent<MeshFilter> ().mesh);
		}
	}

	private void CreateRubble(Vector3 pos, Vector3 scale){
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.AddComponent<Rigidbody> ();

		go.GetComponent<MeshRenderer>().material = stackMat;
		ColorMesh(go.GetComponent<MeshFilter> ().mesh);
	}
	
	// Update is called once per frame
	void Update () {
		if (gameOver)
			return;

		if (Input.GetMouseButtonDown (0)) {
			if (PlaceTile ()) {
				tapSound.Play();
				SpawnTile ();
				scoreCount++;
				scoreText.text = scoreCount.ToString ();
			} else {
				EndGame ();
			}
		}

		MoveTile ();

		//move the stack
		transform.position=Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED*Time.deltaTime);

	}

	private void MoveTile(){

		tileTransition += Time.deltaTime*tileSpeed;
		if(isMovingOnX)
			stack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin(tileTransition)*BOUNDS_SIZE,scoreCount,secondaryPosition);
		else
			stack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition,scoreCount,Mathf.Sin(tileTransition)*BOUNDS_SIZE);
	
	}

	private void SpawnTile(){
		lastTilePostion = stack [stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0) {
			stackIndex = transform.childCount - 1;
		}
		desiredPosition = (Vector3.down) * scoreCount;
		stack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		stack [stackIndex].transform.localScale =new Vector3(stackBounds.x,1,stackBounds.y);

	ColorMesh(stack [stackIndex].GetComponent<MeshFilter> ().mesh);

	}

	private void ColorMesh(Mesh mesh){
	Vector3[] vertices = mesh.vertices;
	Color32[] colors = new Color32[vertices.Length];
	float f = Mathf.Sin (scoreCount*0.25f);

	for(int i=0;i<vertices.Length;i++){
		colors [i] = Lerp4 (gameColors[0],gameColors[1],gameColors[2],gameColors[3],f);
	}

	mesh.colors32 = colors;
}

	private bool PlaceTile(){
		Transform t = stack [stackIndex].transform;

		if (isMovingOnX) {
			float deltaX = lastTilePostion.x- t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN) {
				//cut the tile
				combo = 0;
				stackBounds.x -= Mathf.Abs (deltaX);
				if (stackBounds.x <= 0)
					return false;

				float middle = lastTilePostion.x + t.localPosition.x / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);

				CreateRubble(
					new Vector3((t.position.x > 0)
						? t.position.x + (t.localScale.x/2)
						: t.position.x - (t.localScale.x/2)
						,t.position.y
						,t.position.z),
					new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
				);

				t.localPosition = new Vector3 (middle - (lastTilePostion.x / 2), scoreCount, lastTilePostion.z);

			} else {
				if (combo > COMBO_START_GAIN) {
					stackBounds.x += STACK_BOUNDS_GAIN;

					if (stackBounds.x > BOUNDS_SIZE)
						stackBounds.x = BOUNDS_SIZE;

					float middle = lastTilePostion.x + t.localPosition.x / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (middle - (lastTilePostion.x / 2), scoreCount, lastTilePostion.z);
				}

				combo++;
				t.localPosition = new Vector3 (lastTilePostion.x, scoreCount, lastTilePostion.z);
			}
		}
		else{

			float deltaZ = lastTilePostion.z- t.position.z;
			if (Mathf.Abs (deltaZ) > ERROR_MARGIN) {
				//cut the tile
				combo=0;
				stackBounds.y -= Mathf.Abs (deltaZ);
				if (stackBounds.y <= 0)
					return false;

				float middle = lastTilePostion.z+t.localPosition.z/2;
				t.localScale =new Vector3(stackBounds.x,1,stackBounds.y);

			CreateRubble(
				new Vector3(t.position.x
					,t.position.y
					,(t.position.z > 0)
					? t.position.z + (t.localScale.z/2)
					: t.position.z - (t.localScale.z/2)),
					new Vector3(t.localScale.x,1,Mathf.Abs(deltaZ))
			);


				t.localPosition=new Vector3(lastTilePostion.x,scoreCount,middle-(lastTilePostion.z/2));

			}
			else {
				if (combo > COMBO_START_GAIN) {
					stackBounds.y += STACK_BOUNDS_GAIN;

					if (stackBounds.y > BOUNDS_SIZE)
						stackBounds.y = BOUNDS_SIZE;
					

					float middle = lastTilePostion.z + t.localPosition.z / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition=new Vector3(lastTilePostion.x,scoreCount,middle-(lastTilePostion.z/2));
				}

				combo++;
				t.localPosition = new Vector3 (lastTilePostion.x, scoreCount, lastTilePostion.z);
			}
		}

		secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;

		isMovingOnX = !isMovingOnX;

		return true;
	}

	private Color32 Lerp4(Color32 a,Color32 b, Color32 c, Color32 d, float t){
		if(t<0.33f)
			return Color.Lerp (a,b,t/0.33f);
		else if(t<0.66f)
			return Color.Lerp(b,c,(t-0.33f)/0.33f);
		else 
			return Color.Lerp(b,c,(t-0.66f)/0.66f);
	}

	private void EndGame(){
		if(PlayerPrefs.GetInt("score")<scoreCount)
			PlayerPrefs.SetInt("score",scoreCount);

		gameOver = true;
		Debug.Log ("game over");
		endPanel.SetActive (true);
		//make the tile fall down
		stack[stackIndex].AddComponent<Rigidbody>();
	}

	public void OnButtonClick(string sceneName){
		SceneManager.LoadScene (sceneName);
	}
	
		
}

