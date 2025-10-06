/*
 * Este archivo contiene las notaciones generales sobre el funcionamiento del juego.
 * Está diseñado para servir como referencia rápida para entender cómo interactúan los scripts y los componentes.
 */

/*
 * PlayerController.cs
 * -------------------
 * Controla el movimiento del jugador, el salto y el disparo.
 * - Movimiento: Utiliza Rigidbody2D para aplicar velocidad horizontal.
 * - Salto: Permite un salto único cada vez que el jugador toca una superficie sólida.
 * - Disparo: Instancia balas que se dirigen hacia el cursor del ratón.
 */

/*
 * GameManager.cs
 * --------------
 * Gestiona el estado global del juego.
 * - Puntuación: Incrementa la puntuación al recoger monedas.
 * - Enemigos eliminados: Lleva un conteo de los enemigos derrotados.
 * - Condición de victoria: Comprueba si se han recogido todas las monedas y eliminado todos los enemigos.
 */

/*
 * PlayerHealth.cs
 * ---------------
 * Gestiona la salud del jugador.
 * - Daño: Reduce la salud del jugador al recibir daño.
 * - Restauración: Permite recuperar un porcentaje de la salud máxima al recoger un power-up.
 * - Barra de vida: Actualiza la barra de vida en la interfaz de usuario.
 */

/*
 * Pickable.cs
 * -----------
 * Controla los objetos coleccionables.
 * - Monedas: Incrementan la puntuación global al ser recogidas.
 * - Power-ups: Restauran un porcentaje de la salud del jugador.
 */

/*
 * Enemy.cs
 * --------
 * Controla el comportamiento de los enemigos.
 * - Daño: Inflige daño al jugador al colisionar con él.
 * - Vida: Los enemigos tienen una cantidad de vida que se reduce al recibir daño.
 * - Eliminación: Notifica al GameManager cuando un enemigo es derrotado.
 */

/*
 * EnemySpawner.cs
 * ---------------
 * Genera enemigos en posiciones aleatorias dentro de un área definida.
 * - Intervalo de aparición: Controla el tiempo entre cada aparición.
 * - Configuración: Ajusta la velocidad y dirección inicial de los enemigos generados.
 */

/*
 * EnemyAI.cs
 * ----------
 * Controla la inteligencia artificial de los enemigos.
 * - Movimiento: Los enemigos se mueven hacia el jugador.
 * - Dirección inicial: Permite configurar la dirección inicial del movimiento.
 */

/*
 * CameraFollow.cs
 * ---------------
 * Hace que la cámara siga al jugador.
 * - Desplazamiento: Ajusta la posición de la cámara en los ejes Y y Z.
 */

/*
 * AudioManager.cs
 * ---------------
 * Gestiona los efectos de sonido del juego.
 * - Reproducción: Permite reproducir sonidos específicos como disparos, saltos y recogida de objetos.
 */

/*
 * Bullet.cs
 * ---------
 * Controla el comportamiento de las balas disparadas por el jugador.
 * - Movimiento: Las balas se mueven en la dirección establecida.
 * - Daño: Infligen daño a los enemigos al colisionar con ellos.
 * - Eliminación: Se destruyen al salir de la vista de la cámara o al impactar.
 */

/*
 * General
 * -------
 * - El juego tiene como objetivo recoger todas las monedas y eliminar un número determinado de enemigos.
 * - Los prefabs de los objetos (jugador, enemigos, monedas, power-ups, balas) están configurados para interactuar entre sí mediante los scripts.
 * - La lógica principal del juego está centralizada en el GameManager, mientras que los scripts individuales controlan comportamientos específicos.
 */
