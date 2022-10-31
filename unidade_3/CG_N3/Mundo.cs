﻿#define CG_Gizmo
// #define CG_Privado

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK.Input;
using CG_Biblioteca;

namespace gcgcg
{
    class Mundo : GameWindow
    {
        private static Mundo instanciaMundo = null;

        private float scale;

        private Mundo(int width, int height) : base(width, height) {
            scale = ClientSize.Width / (float)Size.Width;
                }

        public static Mundo GetInstance(int width, int height)
        {
            if (instanciaMundo == null)
                instanciaMundo = new Mundo(width, height);
            return instanciaMundo;
        }

        private CameraOrtho camera = new CameraOrtho();
        protected List<Objeto> objetosLista = new List<Objeto>();
        private ObjetoGeometria objetoSelecionado = null;
        private char objetoId = '@';
        private Ponto4D verticeSelecionado = null;
        private bool bBoxDesenhar = false;
        int mouseX, mouseY;   //TODO: achar método MouseDown para não ter variável Global
        private Poligono objetoNovo = null;
#if CG_Privado
    private Retangulo obj_Retangulo;
    private Privado_SegReta obj_SegReta;
    private Privado_Circulo obj_Circulo;
#endif

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            camera.xmin = 0; camera.xmax = 600; camera.ymin = 0; camera.ymax = 600;

            Console.WriteLine(" --- Ajuda / Teclas: ");
            Console.WriteLine(" [  H     ] mostra teclas usadas. ");

            objetoId = Utilitario.charProximo(objetoId);
            objetoNovo = new Poligono(objetoId, null);
            objetosLista.Add(objetoNovo);
            objetoNovo.PontosAdicionar(new Ponto4D(170, 518));
            objetoNovo.PontosAdicionar(new Ponto4D(401, 517));
            objetoNovo.PontosAdicionar(new Ponto4D(402, 344));
            objetoNovo.PontosAdicionar(new Ponto4D(290, 462));
            objetoNovo.PontosAdicionar(new Ponto4D(170, 356));
            objetoSelecionado = objetoNovo;
            objetoNovo = null;
            objetoSelecionado.ObjetoCor = new Cor(0, 0, 255, 1);

#if CG_Privado
      objetoId = Utilitario.charProximo(objetoId);
      obj_Retangulo = new Retangulo(objetoId, null, new Ponto4D(50, 50, 0), new Ponto4D(150, 150, 0));
      obj_Retangulo.ObjetoCor.CorR = 255; obj_Retangulo.ObjetoCor.CorG = 0; obj_Retangulo.ObjetoCor.CorB = 255;
      objetosLista.Add(obj_Retangulo);
      objetoSelecionado = obj_Retangulo;

      objetoId = Utilitario.charProximo(objetoId);
      obj_SegReta = new Privado_SegReta(objetoId, null, new Ponto4D(50, 150), new Ponto4D(150, 250));
      obj_SegReta.ObjetoCor.CorR = 255; obj_SegReta.ObjetoCor.CorG = 99; obj_SegReta.ObjetoCor.CorB = 71;
      objetosLista.Add(obj_SegReta);
      objetoSelecionado = obj_SegReta;

      objetoId = Utilitario.charProximo(objetoId);
      obj_Circulo = new Privado_Circulo(objetoId, null, new Ponto4D(100, 300), 50);
      obj_Circulo.ObjetoCor.CorR = 177; obj_Circulo.ObjetoCor.CorG = 166; obj_Circulo.ObjetoCor.CorB = 136;
      objetosLista.Add(obj_Circulo);
      objetoSelecionado = obj_Circulo;
#endif
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(camera.xmin, camera.xmax, camera.ymin, camera.ymax, camera.zmin, camera.zmax);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
#if CG_Gizmo
            Sru3D();
#endif
            for (var i = 0; i < objetosLista.Count; i++)
                objetosLista[i].Desenhar();
            if (bBoxDesenhar && (objetoSelecionado != null))
                objetoSelecionado.BBox.Desenhar();
            this.SwapBuffers();
        }

        protected void SelecionarObjeto()
        {
            foreach(Objeto objeto in objetosLista)
            {
                // Encontrar objeto a partir de mouseX e mouseY
            }
        }

        private void AlternarForma(Objeto objeto)
        {
            objeto.PrimitivaTipo = objeto.PrimitivaTipo.Equals(PrimitiveType.LineLoop) ? PrimitiveType.LineStrip : PrimitiveType.LineLoop;
        }

        private void AlternarFormaSelecionado()
        {
            if(objetoNovo != null)
            {
                AlternarForma(objetoNovo);
            }
            else if(objetoSelecionado != null)
            {
                AlternarForma(objetoSelecionado);
            }
        }

        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(e.Key == Key.A)
            {
                SelecionarObjeto();
            }
            if(e.Key == Key.S)
            {
                AlternarFormaSelecionado();
            }
            else if(e.Key == Key.C)
            {
                objetosLista.Remove(objetoSelecionado);
            }
            else if (e.Key == Key.H)
                Utilitario.AjudaTeclado();
            else if (e.Key == Key.Escape)
                Exit();
            else if (e.Key == Key.E)
            {
                Console.WriteLine("--- Objetos / Pontos: ");
                for (var i = 0; i < objetosLista.Count; i++)
                {
                    Console.WriteLine(objetosLista[i]);
                }
            }
            else if (e.Key == Key.O)
                bBoxDesenhar = !bBoxDesenhar;
            else if (e.Key == Key.Enter)
            {
                if (objetoNovo != null)
                {
                    objetoNovo.PontosRemoverUltimo();   // N3-Exe6: "truque" para deixar o rastro
                    objetoSelecionado = objetoNovo;
                    objetoNovo = null;
                }
            }
            else if (e.Key == Key.Space)
            {
                if (objetoNovo == null)
                {
                    objetoId = Utilitario.charProximo(objetoId);
                    objetoNovo = new Poligono(objetoId, null);
                    objetosLista.Add(objetoNovo);
                    objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));
                    objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));  // N3-Exe6: "troque" para deixar o rastro
                }
                else
                    objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));
            }
            else if (objetoSelecionado != null)
            {
                if (e.Key == Key.M)
                    Console.WriteLine(objetoSelecionado.Matriz);
                if (e.Key == Key.V)
                {
                    if (verticeSelecionado == null)
                    {
                        verticeSelecionado = objetoSelecionado.CalculaPontoProximo(new Ponto4D(mouseX, mouseY));
                    }
                    else
                    {
                        verticeSelecionado = null;
                    }
                }
                else if (e.Key == Key.D)
                {
                    // Remover vértice selecionado
                    if (verticeSelecionado != null)
                    {
                        objetoSelecionado.RemoverPonto(verticeSelecionado);
                    }
                }
                else if (e.Key == Key.P)
                    Console.WriteLine(objetoSelecionado);
                else if (e.Key == Key.I)
                    objetoSelecionado.AtribuirIdentidade();
                //TODO: não está atualizando a BBox com as transformações geométricas
                else if (e.Key == Key.Left)
                    objetoSelecionado.TranslacaoXYZ(-10, 0, 0);
                else if (e.Key == Key.Right)
                    objetoSelecionado.TranslacaoXYZ(10, 0, 0);
                else if (e.Key == Key.Up)
                    objetoSelecionado.TranslacaoXYZ(0, 10, 0);
                else if (e.Key == Key.Down)
                    objetoSelecionado.TranslacaoXYZ(0, -10, 0);
                else if (e.Key == Key.PageUp)
                    objetoSelecionado.EscalaXYZ(2, 2, 2);
                else if (e.Key == Key.PageDown)
                    objetoSelecionado.EscalaXYZ(0.5, 0.5, 0.5);
                else if (e.Key == Key.Home)
                    objetoSelecionado.EscalaXYZBBox(0.5, 0.5, 0.5);
                else if (e.Key == Key.End)
                    objetoSelecionado.EscalaXYZBBox(2, 2, 2);
                else if (e.Key == Key.Number1)
                    objetoSelecionado.Rotacao(10);
                else if (e.Key == Key.Number2)
                    objetoSelecionado.Rotacao(-10);
                else if (e.Key == Key.Number3)
                    objetoSelecionado.RotacaoZBBox(10);
                else if (e.Key == Key.Number4)
                    objetoSelecionado.RotacaoZBBox(-10);
                else if (e.Key == Key.Number9)
                    objetoSelecionado = null;                     // desmacar objeto selecionado
                else if (e.Key == Key.R)
                {
                    // Alter a cor do objeto selecionado para vemelho
                    objetoSelecionado.ObjetoCor = new Cor(255, 0, 0, 1);
                }
                else if (e.Key == Key.G)
                {
                    objetoSelecionado.ObjetoCor = new Cor(0, 255, 0, 1);
                }
                else if (e.Key == Key.B)
                {
                    objetoSelecionado.ObjetoCor = new Cor(0, 0, 255, 1);
                }
                else
                    Console.WriteLine(" __ Tecla não implementada.");
            }
            else
                Console.WriteLine(" __ Tecla não implementada.");
        }

        //TODO: não está considerando o NDC
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            // Arrumar escala em telas de DPI elevado
            mouseX = (int) Math.Round(e.Position.X / scale); mouseY = 600 - (int) Math.Round(e.Position.Y / scale); // Inverti eixo Y
            if (objetoNovo != null)
            {
                objetoNovo.PontosUltimo().X = mouseX;
                objetoNovo.PontosUltimo().Y = mouseY;
            }

        }

#if CG_Gizmo
        private void Sru3D()
        {
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.Lines);
            // GL.Color3(1.0f,0.0f,0.0f);
            GL.Color3(Convert.ToByte(255), Convert.ToByte(0), Convert.ToByte(0));
            GL.Vertex3(0, 0, 0); GL.Vertex3(200, 0, 0);
            // GL.Color3(0.0f,1.0f,0.0f);
            GL.Color3(Convert.ToByte(0), Convert.ToByte(255), Convert.ToByte(0));
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 200, 0);
            // GL.Color3(0.0f,0.0f,1.0f);
            GL.Color3(Convert.ToByte(0), Convert.ToByte(0), Convert.ToByte(255));
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 200);
            GL.End();
        }
#endif
    }
    class Program
    {
        static void Main(string[] args)
        {
            Mundo window = Mundo.GetInstance(600, 600);
            window.Title = "CG_N3";
            window.Run(1.0 / 60.0);
        }
    }
}
