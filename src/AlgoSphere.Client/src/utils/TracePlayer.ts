export interface TraceStep {
  s: number; // step
  l?: number; // line
  a: 'cmp' | 'swp' | 'vis' | 'fnd'; // action
  t: number[]; // targets
  v?: Record<string, any>; // variables
}

export interface TraceLog {
  initialState: any[];
  trace: TraceStep[];
}

export class TracePlayer {
  private currentState: any[];
  private trace: TraceStep[];
  private currentIndex: number = -1;
  private onStateChange: (state: any[], currentStep: TraceStep | null) => void;

  constructor(log: TraceLog, onStateChange: (state: any[], currentStep: TraceStep | null) => void) {
    this.currentState = [...log.initialState];
    this.trace = log.trace;
    this.onStateChange = onStateChange;
  }

  public stepForward() {
    if (this.currentIndex >= this.trace.length - 1) return;
    
    this.currentIndex++;
    const step = this.trace[this.currentIndex];
    this.applyStep(step);
    this.onStateChange([...this.currentState], step);
  }

  public stepBackward() {
    if (this.currentIndex < 0) return;
    
    // Đơn giản hóa cho MVP: Chạy lại từ đầu đến bước hiện tại - 1
    // Trong thực tế sẽ dùng snapshot để tối ưu.
    // ... logic rollback ...
  }

  private applyStep(step: TraceStep) {
    if (step.a === 'swp') {
      const [i, j] = step.t;
      const temp = this.currentState[i];
      this.currentState[i] = this.currentState[j];
      this.currentState[j] = temp;
    }
    // Các action khác như 'cmp' hay 'vis' không thay đổi state vật lý mà chỉ thay đổi UI (highlight)
  }

  public getCurrentState() {
    return this.currentState;
  }
}
