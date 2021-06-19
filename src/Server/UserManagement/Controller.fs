module UserManagementController

  open DatabaseRepositories
  open Dto
  open Farmer
  open Microsoft.AspNetCore.Http
  open FSharp.Control.Tasks
  open Saturn

  let playerRepository = PlayerRepository();

  let indexAction (ctx : HttpContext) =
    task {
      let cnf = Controller.getConfig ctx
      let! result =  playerRepository.GetAll ()
      return! Controller.renderHtml ctx (UserManagementViews.index ctx (List.ofSeq (Seq.map Player.fromDomain result)))
    }

  let showAction (ctx: HttpContext) (id : string) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = playerRepository.Get id
      match result with
      | Ok result ->
        // Controller.renderHtml ctx (NotFound.layout)
        return! Controller.renderHtml ctx (UserManagementViews.show ctx (Player.fromDomain result))
      | Error ex ->
        return failwith ex
    }

  let addAction (ctx: HttpContext) =
    task {
      return! Controller.renderHtml ctx (UserManagementViews.add ctx None Map.empty)
    }

  let editAction (ctx: HttpContext) (id : string) =
    task {
      let cnf = Controller.getConfig ctx
      let! result =  playerRepository.Get id
      match result with
      | Ok result ->
        return! Controller.renderHtml ctx (UserManagementViews.edit ctx (Player.fromDomain result) Map.empty)
      | Error ex ->
        return failwith ex
    }


  (*
  let createAction (ctx: HttpContext) =
    task {
      let! input = Controller.getModel<PlayerDto> ctx
      let validateResult = Map.empty
      if validateResult.IsEmpty then

        let cnf = Controller.getConfig ctx
        let! result = Ok //Database.insert cnf.connectionString input
        match result with
        | Ok _ ->
          return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
          return raise ex
      else
        return! Controller.renderHtml ctx (UserManagementViews.add ctx (Some input) validateResult)
    }

  let updateAction (ctx: HttpContext) (id : string) =
    task {
      let! input = Controller.getModel<PlayerDto> ctx
      let validateResult = Map.empty
      if validateResult.IsEmpty then
        let cnf = Controller.getConfig ctx
        let! result =  Ok //Database.update cnf.connectionString input
        match result with
        | Ok _ ->
          return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
          return raise ex
      else
        return! Controller.renderHtml ctx (UserManagementViews.edit ctx input validateResult)
    }

  let deleteAction (ctx: HttpContext) (id : string) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = Ok //Database.delete cnf.connectionString id
      match result with
      | Ok _ ->
        return! Controller.redirect ctx (Links.index ctx)
      | Error ex ->
        return raise ex
    }

    *)

  let resource = controller {
    index indexAction
    show showAction
    (*add addAction
    edit editAction
    create createAction
    update updateAction
    delete deleteAction*)
  }