export interface TodoItem {
  id: number
  task: string
  isCompleted: boolean
  editMode?: boolean
}