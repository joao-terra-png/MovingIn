# CAN-RAT
Minigame no qual seu objetivo é deslocar hamsters de um lado da sala para outro, utilizando de canos com formatos e sentidos diferentes!

## Como funciona?
  Para deslocar os animaizinhos dentro das salas, o jogador dispõe de canos (pipes) de hamsters, e deve enfileirá-los de forma estratégica para conduzir o mamífero.
No entanto, o desafio se torna complexo ao exigir que o usuário reutilize os canos que tem dispostos, dado que não terá quantidade suficiente para usá-los apenas uma vez.
Além disso, o hamster estará sempre em movimento, provocando o jogador a retirar o primeiro cano colocado e inseri-lo ao final. Dessa forma, torna-se essencial a utilização 
de um Tipo Abstrato de Dado (TAD) Fila, com a movimentação de dados - neste caso, os canos - na estrutura First In First Out (FIFO). 

  Os canos possuem lógica de combinação, evitando o encaixe inadequado entre canos não conectáveis, quando tiverem formatos diferentes.
  Já os animais movem-se continuamente até encontrarem um cano curvo ou não identificar nenhum solo a frente.

## Dimensões e ferramentas
  Como todo o projeto, este minigame foi desenvolvido no motor gráfico Unity, em linguagem de programação C#. O minigame se passa em plano paralelo ao plano coordenado XY.

  ## Desenvolvedores
  #### Davi Filipe Del Bianco (858023):
  Responsável pela lógica de movimentação do Hamster e pela lógica de combinação entre canos de formatos diferentes, além da modelagem dos prefabs dos canos.
  #### Felipe Gabriel Rabello ():
  
