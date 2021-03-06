using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

	[Header("Set Dynamically")]
	 public Rigidbody rigid;

	private Neighborhood neighborhood;

	 // Используйте этот метод для инициализации
	 void Awake () {
		rigid = GetComponent<Rigidbody> ();
		neighborhood = GetComponent<Neighborhood>();

		// Выбрать случайную начальную позицию
		pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

		// Выбрать случайную начальную скорость
		Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
		rigid.velocity = vel;

		LookAhead ();

		Color randColor = Color.black;
		while (randColor.r + randColor.g + randColor.b < 1.0f) {
			randColor = new Color (Random.value, Random.value, Random.value);
		}

		Renderer[] rends = gameObject.GetComponentsInChildren<Renderer> ();

		foreach (Renderer r in rends) {
			r.material.color = randColor;
		}
		TrailRenderer tRend = GetComponent<TrailRenderer> ();
		tRend.material.SetColor ("_TintColor", randColor);
	}


	void LookAhead() {
		// Ориентировать птицу клювом в направлении полета
		transform.LookAt(pos + rigid.velocity);

	}
	public Vector3 pos {
		get { return transform.position; }
		set { transform.position = value; }

	}

	void FixedUpdate () {
		Vector3 vel = rigid.velocity; // b
		Spawner spn = Spawner.S; // c

		// ПРЕДОТВРАЩЕНИЕ СТОЛКНОВЕНИЙ - избегать близких соседей
		Vector3 velAvoid = Vector3.zero;
		Vector3 tooClosePos = neighborhood.avgClosePos;
		// Если получен вектор Vector3.zero, ничего предпринимать не надо
		if (tooClosePos != Vector3.zero) {
			velAvoid = pos - tooClosePos;
			velAvoid.Normalize();
			velAvoid *= spn.velocity;
		}
		// СОГЛАСОВАНИЕ СКОРОСТИ - попробовать согласовать скорость с соседями
		Vector3 velAlign = neighborhood.avgVel;
		// Согласование требуется, только если velAlign не равно Vector3.zero
		if (velAlign != Vector3.zero) {
			// Нас интересует только направление, поэтому нормализуем скорость
			velAlign.Normalize();
			// и затем преобразуем в выбранную скорость
			velAlign *= spn.velocity;
		}
		// КОНЦЕНТРАЦИЯ СОСЕДЕЙ - движение в сторону центра группы соседей
		Vector3 velCenter = neighborhood.avgPos;
		if (velCenter != Vector3.zero) {
			velCenter -= transform.position;
			velCenter.Normalize();
			velCenter *= spn.velocity;
		}

		// ПРИТЯЖЕНИЕ - организовать движение в сторону объекта Attractor
		Vector3 delta = Attractor.POS - pos; // d
		// Проверить, куда двигаться, в сторону Attractor или от него
		bool attracted = (delta.magnitude > spn.attractPushDist);
		Vector3 velAttract = delta.normalized * spn.velocity; // e
		// Применить все скорости
		float fdt = Time.fixedDeltaTime;

		if (velAvoid != Vector3.zero) {
			vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid*fdt);
		} else {
			if (velAlign != Vector3.zero) {
				vel = Vector3.Lerp(vel, velAlign, spn.velMatching*fdt);
			}
			if (velCenter != Vector3.zero) {
				vel = Vector3.Lerp(vel, velAlign, spn.flockCentering*fdt);
			}
			if (velAttract != Vector3.zero) {
				if (attracted) {
					vel = Vector3.Lerp(vel, velAttract, spn.attractPull*fdt);
				} else {
					vel = Vector3.Lerp(vel, -velAttract, spn.attractPull*fdt);
				}
			}
		}

	
		// Установить vel в соответствии c velocity в объекте-одиночке Spawner
		vel = vel.normalized * spn.velocity;
		// В заключение присвоить скорость компоненту Rigidbody
		rigid.velocity = vel;
		LookAhead();
	}
}
