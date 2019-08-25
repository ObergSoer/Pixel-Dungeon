var walkSpeed : float = 1.0;
var runSpeed : float = 2.0;
var gridSize : int = 1;
enum Orientation {Horizontal, Vertical}
var gridOrientation = Orientation.Horizontal;
var allowDiagonals = false;
var correctDiagonalSpeed = true;
private var input = Vector2.zero;
 
function Start () {
	var myTransform = transform;
	var startPosition : Vector3;
	var endPosition : Vector3;
	var t : float;
	var tx : float;
	var moveSpeed = walkSpeed;
 
	while (true) {
		while (input == Vector2.zero) {
			GetAxes();
			tx = 0.0;
			yield;
		}
 
		transform.forward = Vector3.Normalize(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")));
		startPosition = myTransform.position;
		endPosition = gridOrientation == Orientation.Horizontal?
			Vector3(Mathf.Round(myTransform.position.x), 0.0, Mathf.Round(myTransform.position.z)) +
			Vector3(System.Math.Sign(input.x)*gridSize, 0.0, System.Math.Sign(input.y)*gridSize)
			:
			Vector3(Mathf.Round(myTransform.position.x), Mathf.Round(myTransform.position.y), 0.0) +
			Vector3(System.Math.Sign(input.x)*gridSize, System.Math.Sign(input.y)*gridSize, 0.0);
		t = tx;
		while (t < 1.0) {
			moveSpeed = Input.GetButton("Run")? runSpeed : walkSpeed;
			t += Time.deltaTime * (moveSpeed/gridSize) * (correctDiagonalSpeed && input.x != 0.0 && input.y != 0.0? .7071 : 1.0);
			myTransform.position = Vector3.Lerp(startPosition, endPosition, t);
			yield;
		}
		tx = t - 1.0;	// Used to prevent slight visual hiccups on "grid lines" due to Time.deltaTime variance
		GetAxes();
	}
}
 
function GetAxes () {
	input = Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	if (allowDiagonals)
		return;
	if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
		input.y = 0.0;
	else
		input.x = 0.0;
}