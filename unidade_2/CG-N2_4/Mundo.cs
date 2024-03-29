﻿/**
  Autor: Dalton Solano dos Reis
**/

//#define CG_Privado // código do professor.
#define CG_Gizmo  // debugar gráfico.
#define CG_Debug // debugar texto.
#define CG_OpenGL // render OpenGL.
//#define CG_DirectX // render DirectX.

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

    private Mundo(int width, int height) : base(width, height) { }

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
    private bool bBoxDesenhar = false;
    int mouseX, mouseY;   //TODO: achar método MouseDown para não ter variável Global
    private bool mouseMoverPto = false;
    private Retangulo obj_Retangulo;
#if CG_Privado
    private Privado_SegReta obj_SegReta;
    private Privado_Circulo obj_Circulo;
#endif

    private Ponto4D esquerdaSup = new Ponto4D(-200, 200);
    private Ponto4D direitaSup = new Ponto4D(200, 200);
    private Ponto4D direitaInf = new Ponto4D(200, -200);
    private Ponto4D esquerdaInf = new Ponto4D(-200, -200);
    private static PrimitiveType[] _tiposPrimitivos = {
      PrimitiveType.Points,
      PrimitiveType.Lines,
      PrimitiveType.LineLoop,
      PrimitiveType.LineStrip,
      PrimitiveType.Triangles,
      PrimitiveType.TriangleStrip,
      PrimitiveType.TriangleFan,
      PrimitiveType.Quads,
      PrimitiveType.QuadStrip,
      PrimitiveType.Polygon
    };

    private LinkedList<PrimitiveType> tiposPrimitivos = new LinkedList<PrimitiveType>(_tiposPrimitivos);
    private PrimitiveType primitivaAtual;

    private void AlternarPrimitiva() {
      PrimitiveType primeira = tiposPrimitivos.First.Value;
      tiposPrimitivos.RemoveFirst();
      primitivaAtual = tiposPrimitivos.First.Value;
      tiposPrimitivos.AddLast(primeira);
    }


    private void DesenharPrimitivas() {
      GL.Color3(Convert.ToByte(0), Convert.ToByte(0), Convert.ToByte(0));
      GL.LineWidth(10);
      GL.PointSize(10);
      GL.Begin(primitivaAtual);
      GL.Vertex2(esquerdaSup.X, esquerdaSup.Y);
      GL.Vertex2(direitaSup.X, direitaSup.Y);
      GL.Vertex2(direitaInf.X, direitaInf.Y);
      GL.Vertex2(esquerdaInf.X, esquerdaInf.Y);
      GL.End();
    }

    private void DesenharCirculo(Ponto4D pontoCentral, int raio, Cor cor, int tamanho, int pontos = 72, PrimitiveType primitivo = PrimitiveType.Points)
    {
      Circulo circulo = new Circulo(Convert.ToChar("C"), null, pontoCentral, raio, pontos, primitivo);
      circulo.ObjetoCor.CorR = cor.CorR; circulo.ObjetoCor.CorG = cor.CorG; circulo.ObjetoCor.CorB = cor.CorB;
      circulo.PrimitivaTamanho = tamanho;
      objetosLista.Add(circulo);
    }

    private void DesenharReta(Ponto4D pontoInicio, Ponto4D pontoFim, Cor cor, int tamanho) {
      SegReta reta = new SegReta(Convert.ToChar("R"), null, pontoInicio, pontoFim);
      reta.ObjetoCor.CorR = cor.CorR; reta.ObjetoCor.CorG = cor.CorG; reta.ObjetoCor.CorB = cor.CorB;
      reta.PrimitivaTamanho = tamanho;
      objetosLista.Add(reta);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      camera.xmin = -400; camera.xmax = 400; camera.ymin = -400; camera.ymax = 400;

      Console.WriteLine(" --- Ajuda / Teclas: ");
      Console.WriteLine(" [  H     ] mostra teclas usadas. ");

#if CG_Privado
      objetoId = Utilitario.charProximo(objetoId);
      obj_SegReta = new Privado_SegReta(objetoId, null, new Ponto4D(50, 150), new Ponto4D(150, 250));
      obj_SegReta.ObjetoCor.CorR = 255; obj_SegReta.ObjetoCor.CorG = 255; obj_SegReta.ObjetoCor.CorB = 0;
      objetosLista.Add(obj_SegReta);
      objetoSelecionado = obj_SegReta;

      objetoId = Utilitario.charProximo(objetoId);
      obj_Circulo = new Privado_Circulo(objetoId, null, new Ponto4D(100, 300), 50);
      obj_Circulo.ObjetoCor.CorR = 0; obj_Circulo.ObjetoCor.CorG = 255; obj_Circulo.ObjetoCor.CorB = 255;
      obj_Circulo.PrimitivaTipo = PrimitiveType.Points;
      obj_Circulo.PrimitivaTamanho = 5;
      objetosLista.Add(obj_Circulo);
      objetoSelecionado = obj_Circulo;

#endif
#if CG_OpenGL
      GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
#endif
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);
#if CG_OpenGL
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();
      GL.Ortho(camera.xmin, camera.xmax, camera.ymin, camera.ymax, camera.zmin, camera.zmax);
      DesenharPrimitivas();
#endif
    }
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
#if CG_OpenGL
      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadIdentity();
      DesenharPrimitivas();
#endif
#if CG_Gizmo      
      Sru3D();
#endif
      for (var i = 0; i < objetosLista.Count; i++)
        objetosLista[i].Desenhar();
#if CG_Gizmo
      if (bBoxDesenhar && (objetoSelecionado != null))
        objetoSelecionado.BBox.Desenhar();
#endif
      this.SwapBuffers();
    }

    protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
    {
      if (e.Key == Key.H)
        Utilitario.AjudaTeclado();
      else if (e.Key == Key.Escape)
        Exit();
      else if (e.Key == Key.E)
        camera.PanEsquerda();
      else if (e.Key == Key.D)
        camera.PanDireita();
      else if (e.Key == Key.C)
        camera.PanCima();
      else if (e.Key == Key.B)
        camera.PanBaixo();
      else if (e.Key == Key.I)
        camera.ZoomIn();
      else if (e.Key == Key.O)
        camera.ZoomOut();
      else if (e.Key == Key.Space)
        AlternarPrimitiva();
#if CG_Gizmo
      else if (e.Key == Key.O)
        bBoxDesenhar = !bBoxDesenhar;
#endif
      else if (e.Key == Key.V)
        mouseMoverPto = !mouseMoverPto;   //TODO: falta atualizar a BBox do objeto
      else
        Console.WriteLine(" __ Tecla não implementada.");
    }

    //TODO: não está considerando o NDC
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      mouseX = e.Position.X; mouseY = 600 - e.Position.Y; // Inverti eixo Y
      if (mouseMoverPto && (objetoSelecionado != null))
      {
        objetoSelecionado.PontosUltimo().X = mouseX;
        objetoSelecionado.PontosUltimo().Y = mouseY;
      }
    }

#if CG_Gizmo
    private void Sru3D()
    {
#if CG_OpenGL
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
#endif
    }
#endif    
  }
  class Program
  {
    static void Main(string[] args)
    {
      ToolkitOptions.Default.EnableHighResolution = false;
      Mundo window = Mundo.GetInstance(600, 600);
      window.Title = "CG_N2";
      window.Run(1.0 / 60.0);
    }
  }
}
