/*
 * Este archivo contiene las notaciones generales sobre el funcionamiento del juego.
 * Est� dise�ado para servir como referencia r�pida para entender c�mo interact�an los scripts y los componentes.
 */

/*
 * PlayerController.cs
 * -------------------
 * Controla el movimiento del jugador, el salto y el disparo.
 * - Movimiento: Utiliza Rigidbody2D para aplicar velocidad horizontal.
 * - Salto: Permite un salto �nico cada vez que el jugador toca una superficie s�lida.
 * - Disparo: Instancia balas que se dirigen hacia el cursor del rat�n.
 */

/*
 * GameManager.cs
 * --------------
 * Gestiona el estado global del juego.
 * - Puntuaci�n: Incrementa la puntuaci�n al recoger monedas.
 * - Enemigos eliminados: Lleva un conteo de los enemigos derrotados.
 * - Condici�n de victoria: Comprueba si se han recogido todas las monedas y eliminado todos los enemigos.
 */

/*
 * PlayerHealth.cs
 * ---------------
 * Gestiona la salud del jugador.
 * - Da�o: Reduce la salud del jugador al recibir da�o.
 * - Restauraci�n: Permite recuperar un porcentaje de la salud m�xima al recoger un power-up.
 * - Barra de vida: Actualiza la barra de vida en la interfaz de usuario.
 */

/*
 * Pickable.cs
 * -----------
 * Controla los objetos coleccionables.
 * - Monedas: Incrementan la puntuaci�n global al ser recogidas.
 * - Power-ups: Restauran un porcentaje de la salud del jugador.
 */

/*
 * Enemy.cs
 * --------
 * Controla el comportamiento de los enemigos.
 * - Da�o: Inflige da�o al jugador al colisionar con �l.
 * - Vida: Los enemigos tienen una cantidad de vida que se reduce al recibir da�o.
 * - Eliminaci�n: Notifica al GameManager cuando un enemigo es derrotado.
 */

/*
 * EnemySpawner.cs
 * ---------------
 * Genera enemigos en posiciones aleatorias dentro de un �rea definida.
 * - Intervalo de aparici�n: Controla el tiempo entre cada aparici�n.
 * - Configuraci�n: Ajusta la velocidad y direcci�n inicial de los enemigos generados.
 */

/*
 * EnemyAI.cs
 * ----------
 * Controla la inteligencia artificial de los enemigos.
 * - Movimiento: Los enemigos se mueven hacia el jugador.
 * - Direcci�n inicial: Permite configurar la direcci�n inicial del movimiento.
 */

/*
 * CameraFollow.cs
 * ---------------
 * Hace que la c�mara siga al jugador.
 * - Desplazamiento: Ajusta la posici�n de la c�mara en los ejes Y y Z.
 */

/*
 * AudioManager.cs
 * ---------------
 * Gestiona los efectos de sonido del juego.
 * - Reproducci�n: Permite reproducir sonidos espec�ficos como disparos, saltos y recogida de objetos.
 */

/*
 * Bullet.cs
 * ---------
 * Controla el comportamiento de las balas disparadas por el jugador.
 * - Movimiento: Las balas se mueven en la direcci�n establecida.
 * - Da�o: Infligen da�o a los enemigos al colisionar con ellos.
 * - Eliminaci�n: Se destruyen al salir de la vista de la c�mara o al impactar.
 */

/*
 * General
 * -------
 * - El juego tiene como objetivo recoger todas las monedas y eliminar un n�mero determinado de enemigos.
 * - Los prefabs de los objetos (jugador, enemigos, monedas, power-ups, balas) est�n configurados para interactuar entre s� mediante los scripts.
 * - La l�gica principal del juego est� centralizada en el GameManager, mientras que los scripts individuales controlan comportamientos espec�ficos.
 */
