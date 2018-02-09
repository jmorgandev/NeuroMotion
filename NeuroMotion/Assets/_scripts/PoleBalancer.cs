using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoleBalancer : NeuroMotion.Subject {

    public float HorizontalLimit = 6.0f;
    public int   AngularLimit    = 45;
    public float MotorForce      = 5.0f;

    public float timeEmphasis = 1.0f;
    public float balanceEmphasis = 1.0f;

    private float dotProductRange = 0.0f;

    private Rigidbody2D paddle = null;
    private Rigidbody2D pole   = null;

    private List<float> sensors = null;
    private List<float> outputs = null;

    private const int INPUT_COUNT = 4;
    private const int ANGLE_LIMIT = 90;

    //Pretty much an enum except I would rather not have to typecast to an int when using
    //these for array indexing...
    private const int PADDLE_POS      = 0;
    private const int PADDLE_VELOCITY = 1;
    private const int POLE_ANGLE      = 2;
    private const int POLE_VELOCITY   = 3;

    private const int LEFT_MOTOR  = 0;
    private const int RIGHT_MOTOR = 1;
    private const int MOTOR = 0;

    private void Awake() {
        sensors = new List<float>(new float[INPUT_COUNT]);
    }

    // Use this for initialization
    void Start () {
        paddle = transform.Find("paddle").GetComponent<Rigidbody2D>();
        pole = transform.Find("pole").GetComponent<Rigidbody2D>();
        pole.AddTorque(Random.Range(-0.2f, 0.2f), ForceMode2D.Impulse);

        dotProductRange = 1f - ((float)AngularLimit / ANGLE_LIMIT);
	}

    private void FixedUpdate() {
        //Failure condition
        if (Vector2.Dot(pole.transform.up, Vector2.up) < dotProductRange ||
           paddle.position.magnitude >= HorizontalLimit) {
            DisableSubject();
        }
        else {
            sensors[PADDLE_POS] = paddle.position.x;// / (HorizontalLimit * 0.5f);
            sensors[PADDLE_VELOCITY] = paddle.velocity.x * 0.1f;
            sensors[POLE_ANGLE] = Vector2.Dot(pole.transform.up, Vector2.right) * 2.0f;
            //@todo change from angularVelocity to some form of delta?
            sensors[POLE_VELOCITY] = pole.angularVelocity * 0.1f;

            outputs = neuralNetwork.FeedForward(sensors);

            if (outputs.Count == 2) {
                float left = outputs[LEFT_MOTOR];
                float right = outputs[RIGHT_MOTOR];
                //paddle.velocity = new Vector2((right - left) * MotorForce, 0.0f);
                paddle.AddForce(new Vector2(right - left * MotorForce, 0.0f), ForceMode2D.Impulse);
            }
            else if (outputs.Count == 1) {
                //paddle.velocity = new Vector2(outputs[MOTOR] * MotorForce, 0.0f);
                paddle.AddForce(new Vector2(outputs[MOTOR] * MotorForce, 0.0f), ForceMode2D.Impulse);
            }
        }
    }

    public override void UpdateFitness() {
        fitness += (Time.deltaTime * timeEmphasis);
        fitness += (Vector2.Dot(pole.transform.up, Vector2.up) * balanceEmphasis);
    }
}
