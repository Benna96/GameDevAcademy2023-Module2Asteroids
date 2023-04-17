using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class PlayerController : WrapBehaviour
{
    [field: SerializeField] private float acceleration { get; set; } = 3f;
    [field: SerializeField] private float maxForwardsSpeed { get; set; } = 2.5f;
    [field: SerializeField] private float backwardsSpeedMult { get; set; } = 0.5f;
    private float maxBackwardsSpeed { get; set; } = 1f;

    private Rigidbody2D rb { get; set; }
    private ParticleSystem[] particlesOfThisAndGhosts { get; set; }

    private float turnInput { get; set; }
    private float thrustInput { get; set; }

    private bool _isInvincible = false;
    public bool isInvincible
    {
        get { return _isInvincible; }
        set
        {
            if (_isInvincible == value)
                return;

            _isInvincible = value;

            var collider = GetComponent<Collider2D>();
            collider.enabled = !value;
        }
    }
    public float iframeTimeRemaining { get; set; }

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        maxBackwardsSpeed = backwardsSpeedMult * maxForwardsSpeed;

        particlesOfThisAndGhosts = new ParticleSystem[1+ghosts.Length];
        particlesOfThisAndGhosts[0] = GetComponent<ParticleSystem>();
        for (int i = 0; i < ghosts.Length; i++)
            particlesOfThisAndGhosts[1+i] = ghosts[i].GetComponent<ParticleSystem>();
    }

    protected override void AddToLevelManager()
    {
        if (GameManager.instance != null)
            GameManager.instance.player = this;
    }
    protected override void RemoveFromLevelManager()
    {
        if (GameManager.instance != null)
            GameManager.instance.player = null;
    }

    private void FixedUpdate() {
        var delta = Time.fixedDeltaTime;
        Vector2 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        bool overMax = localVelocity.y >= maxForwardsSpeed;
        bool overBackwardsMax = -localVelocity.y >= maxBackwardsSpeed;

        if (thrustInput != 0f)
            Accelerate(delta, localVelocity, overMax, overBackwardsMax, thrustInput > 0);
            
        void Accelerate(float delta, Vector2 localVelocity, bool overMax, bool overBackwardsMax, bool forwards = true)
        {
            if ((forwards && overMax) || (!forwards && overBackwardsMax))
                return;

            float accelerationMult = forwards ? 1f : backwardsSpeedMult;
            rb.AddForce(delta * 50f * thrustInput * acceleration * accelerationMult * transform.up);
        }
    }

    protected override void Update()
    {
        if (iframeTimeRemaining > 0f)
        {
            iframeTimeRemaining -= Time.deltaTime;
            if (iframeTimeRemaining <= 0f)
                isInvincible = false;
        }

        if (turnInput != 0f)
            gameObject.transform.RotateAround(
                transform.position,
                transform.forward,
                -turnInput * Time.deltaTime * 150f);

        base.Update();

        if (thrustInput > 0 && !particlesOfThisAndGhosts[0].isEmitting)
            foreach (var particles in particlesOfThisAndGhosts)
                particles.Play();
        else if (thrustInput == 0 && particlesOfThisAndGhosts[0].isPlaying)
            foreach (var particles in particlesOfThisAndGhosts)
                particles.Stop();
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnTurn(InputValue value) => turnInput = value.Get<float>();
    private void OnThrust(InputValue value) => thrustInput = value.Get<float>();
#pragma warning restore IDE0051 // Remove unused private members

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("Enemy"))
            return;
        
        if (GameManager.instance != null)
            GameManager.instance.UpdateLives(-1);
    }

    public void BecomeTemporarilyInvisible(float duration) => StartCoroutine(BecomeTemporarilyInvisible_(duration));
    private IEnumerator BecomeTemporarilyInvisible_(float duration)
    {
        isInvincible = true;
        iframeTimeRemaining = duration;
        var renderer = GetComponent<SpriteShapeRenderer>();
        var r = renderer.color.r;
        var g = renderer.color.g;
        var b = renderer.color.b;
        var a = renderer.color.a;
        Color modifiedColor = new(r, g, b, a);

        while (isInvincible)
        {
            var diffFromHalfSecondNormalized = Mathf.Abs((float)((DateTime.Now.TimeOfDay.TotalSeconds % 1) - 0.5)) / 0.5f;
            modifiedColor.a = Mathf.Lerp(0.3f, 1f, diffFromHalfSecondNormalized);
            renderer.color = modifiedColor;
            yield return null;
        }

        renderer.color = new(r, g, b, a);
    }
}
