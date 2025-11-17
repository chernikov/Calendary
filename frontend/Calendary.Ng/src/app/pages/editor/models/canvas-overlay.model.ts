export type CanvasElementType = 'text' | 'rectangle' | 'circle' | 'line';

export interface CanvasElementBase {
  id: string;
  type: CanvasElementType;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  fill: string;
  stroke: string;
  strokeWidth: number;
  opacity: number;
}

export interface TextCanvasElement extends CanvasElementBase {
  type: 'text';
  text: string;
  fontFamily: string;
  fontSize: number;
  fontWeight: 'normal' | 'bold';
  fontStyle: 'normal' | 'italic';
  underline: boolean;
  align: 'left' | 'center' | 'right';
}

export type ShapeCanvasElement = CanvasElementBase & { type: 'rectangle' | 'circle' | 'line' };

export type CanvasElement = TextCanvasElement | ShapeCanvasElement;
